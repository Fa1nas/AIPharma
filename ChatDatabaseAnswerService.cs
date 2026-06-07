using AIPharma.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AIPharma.Services
{
    public class ChatDatabaseAnswerService
    {
        private readonly ApplicationDbContext _db;

        public ChatDatabaseAnswerService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string?> TryAnswerAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return null;
            }

            var normalizedQuestion = Normalize(question);

            var productAnswer = await TryAnswerProductAvailabilityAsync(question, normalizedQuestion);

            if (!string.IsNullOrWhiteSpace(productAnswer))
            {
                return productAnswer;
            }

            var pharmacyAnswer = await TryAnswerPharmacyAsync(question, normalizedQuestion);

            if (!string.IsNullOrWhiteSpace(pharmacyAnswer))
            {
                return pharmacyAnswer;
            }

            var discountAnswer = await TryAnswerDiscountsAsync(question, normalizedQuestion);

            if (!string.IsNullOrWhiteSpace(discountAnswer))
            {
                return discountAnswer;
            }

            return null;
        }

        private async Task<string?> TryAnswerProductAvailabilityAsync(string originalQuestion, string normalizedQuestion)
        {
            var productQuestionWords = new[]
            {
                "є", "е", "наявн", "скільки", "сколько", "залиш", "остат", "купити", "купить",
                "де є", "где есть", "товар", "препарат", "ліки", "лекарство"
            };

            var isProductQuestion = productQuestionWords.Any(w => normalizedQuestion.Contains(w));

            if (!isProductQuestion)
            {
                return null;
            }

            var products = await _db.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductStocks)
                    .ThenInclude(s => s.Pharmacy)
                .ToListAsync();

            var product = products
                .Select(p => new
                {
                    Product = p,
                    Score = CalculateProductScore(normalizedQuestion, p.Name, p.Manufacturer, p.Description)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Product)
                .FirstOrDefault();

            if (product == null)
            {
                return null;
            }

            var stocks = product.ProductStocks
                .Where(s => s.Quantity > 0)
                .OrderByDescending(s => s.Quantity)
                .ToList();

            var priceText = product.DiscountPrice.HasValue
                ? $"{product.DiscountPrice.Value:0.00} грн зі знижкою замість {product.Price:0.00} грн"
                : $"{product.Price:0.00} грн";

            if (!stocks.Any())
            {
                return $"Товар «{product.Name}» є в каталозі AIPharma, але зараз відсутній у наявності в аптеках. Орієнтовна ціна товару: {priceText}.";
            }

            var builder = new StringBuilder();

            builder.AppendLine($"Товар «{product.Name}» є в наявності.");
            builder.AppendLine($"Категорія: {product.ProductCategory?.Name ?? "не вказано"}.");
            builder.AppendLine($"Виробник: {product.Manufacturer}.");
            builder.AppendLine($"Ціна: {priceText}.");
            builder.AppendLine();
            builder.AppendLine("Наявність за аптеками:");

            foreach (var stock in stocks.Take(5))
            {
                builder.AppendLine($"- {stock.Pharmacy.Name}, {stock.Pharmacy.Address} — {stock.Quantity} шт.");
            }

            if (stocks.Count > 5)
            {
                builder.AppendLine($"Також товар є ще у {stocks.Count - 5} аптеках мережі.");
            }

            builder.AppendLine();
            builder.AppendLine("Для оформлення замовлення відкрийте сторінку товару в каталозі та оберіть зручну аптеку.");

            return builder.ToString();
        }

        private async Task<string?> TryAnswerPharmacyAsync(string originalQuestion, string normalizedQuestion)
        {
            var pharmacyWords = new[]
            {
                "аптека", "аптек", "адрес", "де знаход", "где находится", "поруч", "рядом",
                "найближ", "ближай", "карта", "мапа", "центр", "калинов", "перемог", "тополя",
                "парус", "соняч", "робоч", "слобожан", "яворницьк", "набереж"
            };

            var isPharmacyQuestion = pharmacyWords.Any(w => normalizedQuestion.Contains(w));

            if (!isPharmacyQuestion)
            {
                return null;
            }

            var pharmacies = await _db.Pharmacies
                .OrderBy(p => p.Name)
                .ToListAsync();

            if (!pharmacies.Any())
            {
                return null;
            }

            var matchedPharmacies = pharmacies
                .Select(p => new
                {
                    Pharmacy = p,
                    Score = CalculatePharmacyScore(normalizedQuestion, p.Name, p.Address)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Pharmacy)
                .Take(3)
                .ToList();

            if (!matchedPharmacies.Any())
            {
                var builderAll = new StringBuilder();

                builderAll.AppendLine("У мережі AIPharma доступні аптеки в різних районах міста.");
                builderAll.AppendLine("Найзручніше переглянути їх розташування на сторінці «Аптеки», де додана інтерактивна Google-карта.");
                builderAll.AppendLine();
                builderAll.AppendLine("Декілька аптек мережі:");

                foreach (var pharmacy in pharmacies.Take(5))
                {
                    builderAll.AppendLine($"- {pharmacy.Name}, {pharmacy.Address}");
                }

                return builderAll.ToString();
            }

            var builder = new StringBuilder();

            builder.AppendLine("За вашим запитом найбільше підходять такі аптеки AIPharma:");
            builder.AppendLine();

            foreach (var pharmacy in matchedPharmacies)
            {
                builder.AppendLine($"- {pharmacy.Name}");
                builder.AppendLine($"  Адреса: {pharmacy.Address}");
                builder.AppendLine($"  Телефон: {pharmacy.Phone}");
                builder.AppendLine($"  Графік роботи: {pharmacy.WorkSchedule}");
                builder.AppendLine();
            }

            builder.AppendLine("Для точнішого вибору можна також переглянути розташування аптек на карті у вкладці «Аптеки».");

            return builder.ToString();
        }

        private async Task<string?> TryAnswerDiscountsAsync(string originalQuestion, string normalizedQuestion)
        {
            var discountWords = new[]
            {
                "знижк", "скидк", "акці", "акци", "дешев", "дешевле", "спецпропози"
            };

            var isDiscountQuestion = discountWords.Any(w => normalizedQuestion.Contains(w));

            if (!isDiscountQuestion)
            {
                return null;
            }

            var discountedProducts = await _db.Products
                .Include(p => p.ProductCategory)
                .Where(p => p.DiscountPrice != null)
                .OrderBy(p => p.DiscountPrice)
                .Take(7)
                .ToListAsync();

            if (!discountedProducts.Any())
            {
                return "Зараз у каталозі немає товарів зі знижкою. Актуальні пропозиції можна переглянути у вкладці «Знижки».";
            }

            var builder = new StringBuilder();

            builder.AppendLine("Зараз у AIPharma є такі товари зі знижкою:");
            builder.AppendLine();

            foreach (var product in discountedProducts)
            {
                builder.AppendLine($"- {product.Name} — {product.DiscountPrice:0.00} грн замість {product.Price:0.00} грн.");
            }

            builder.AppendLine();
            builder.AppendLine("Повний список акційних товарів можна переглянути у вкладці «Знижки».");

            return builder.ToString();
        }

        private int CalculateProductScore(string question, string name, string manufacturer, string description)
        {
            var score = 0;

            var normalizedName = Normalize(name);
            var normalizedManufacturer = Normalize(manufacturer);
            var normalizedDescription = Normalize(description);

            if (question.Contains(normalizedName))
            {
                score += 100;
            }

            foreach (var word in normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 4 && question.Contains(word))
                {
                    score += 20;
                }
            }

            foreach (var word in normalizedManufacturer.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 4 && question.Contains(word))
                {
                    score += 8;
                }
            }

            foreach (var word in normalizedDescription.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 6 && question.Contains(word))
                {
                    score += 3;
                }
            }

            return score;
        }

        private int CalculatePharmacyScore(string question, string name, string address)
        {
            var score = 0;

            var normalizedName = Normalize(name);
            var normalizedAddress = Normalize(address);

            if (question.Contains(normalizedName))
            {
                score += 100;
            }

            foreach (var word in normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 3 && question.Contains(word))
                {
                    score += 15;
                }
            }

            foreach (var word in normalizedAddress.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 4 && question.Contains(word))
                {
                    score += 20;
                }
            }

            return score;
        }

        private string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text
                .ToLower()
                .Replace("ё", "е")
                .Replace("і", "и")
                .Replace("ї", "и")
                .Replace("є", "е")
                .Replace("’", "")
                .Replace("'", "")
                .Replace(".", " ")
                .Replace(",", " ")
                .Replace(":", " ")
                .Replace(";", " ")
                .Replace("!", " ")
                .Replace("?", " ")
                .Trim();
        }
    }
}