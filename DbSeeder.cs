using AIPharma.Models;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            db.Database.Migrate();

            SeedRoles(db);
            SeedUsers(db);
            SeedCategories(db);
            SeedPharmacies(db);
            SeedProducts(db);
            SeedStocks(db);
            SeedAiData(db);
            SeedEmployees(db);
        }

        private static void SeedRoles(ApplicationDbContext db)
        {
            if (!db.Roles.Any(r => r.Name == "Admin"))
            {
                db.Roles.Add(new Role { Name = "Admin" });
            }

            if (!db.Roles.Any(r => r.Name == "User"))
            {
                db.Roles.Add(new Role { Name = "User" });
            }

            if (!db.Roles.Any(r => r.Name == "Manager"))
            {
                db.Roles.Add(new Role { Name = "Manager" });
            }

            db.SaveChanges();
        }

        private static void SeedUsers(ApplicationDbContext db)
        {
            var adminRole = db.Roles.First(x => x.Name == "Admin");
            var userRole = db.Roles.First(x => x.Name == "User");
            var managerRole = db.Roles.First(x => x.Name == "Manager");

            if (!db.Users.Any(u => u.Email == "admin@aipharma.com"))
            {
                db.Users.Add(new User
                {
                    FullName = "Адміністратор AIPharma",
                    Email = "admin@aipharma.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    RoleId = adminRole.Id
                });
            }

            if (!db.Users.Any(u => u.Email == "user@aipharma.com"))
            {
                db.Users.Add(new User
                {
                    FullName = "Тестовий користувач",
                    Email = "user@aipharma.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                    RoleId = userRole.Id
                });
            }

            var firstPharmacy = db.Pharmacies.OrderBy(p => p.Id).FirstOrDefault();

            if (firstPharmacy != null && !db.Users.Any(u => u.Email == "manager1@aipharma.com"))
            {
                db.Users.Add(new User
                {
                    FullName = "Менеджер аптеки №1",
                    Email = "manager1@aipharma.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    RoleId = managerRole.Id,
                    PharmacyId = firstPharmacy.Id
                });
            }

            db.SaveChanges();
        }

        private static void SeedCategories(ApplicationDbContext db)
        {
            if (db.ProductCategories.Any()) return;

            db.ProductCategories.AddRange(
                new ProductCategory { Name = "Знеболювальні" },
                new ProductCategory { Name = "Противірусні" },
                new ProductCategory { Name = "Вітаміни" },
                new ProductCategory { Name = "Засоби від застуди" },
                new ProductCategory { Name = "Догляд та гігієна" },
                new ProductCategory { Name = "Шлунок та травлення" },
                new ProductCategory { Name = "Серце та тиск" },
                new ProductCategory { Name = "Дитячі товари" }
            );

            db.SaveChanges();
        }

        private static void SeedPharmacies(ApplicationDbContext db)
        {
            if (db.Pharmacies.Any()) return;

            db.Pharmacies.AddRange(
                new Pharmacy { Name = "AIPharma №1 Центр", Address = "м. Дніпро, просп. Дмитра Яворницького, 50", Phone = "+380 95 111 11 11", WorkSchedule = "08:00 - 21:00", Latitude = 48.4647, Longitude = 35.0462 },
                new Pharmacy { Name = "AIPharma №2 Нагірний", Address = "м. Дніпро, вул. Січеславська Набережна, 29", Phone = "+380 95 111 11 12", WorkSchedule = "08:00 - 21:00", Latitude = 48.4685, Longitude = 35.0593 },
                new Pharmacy { Name = "AIPharma №3 Перемога", Address = "м. Дніпро, ж/м Перемога, 45", Phone = "+380 95 111 11 13", WorkSchedule = "08:00 - 22:00", Latitude = 48.4157, Longitude = 35.0724 },
                new Pharmacy { Name = "AIPharma №4 Тополя", Address = "м. Дніпро, ж/м Тополя-2, 18", Phone = "+380 95 111 11 14", WorkSchedule = "08:00 - 21:00", Latitude = 48.4079, Longitude = 35.0021 },
                new Pharmacy { Name = "AIPharma №5 Лівобережна", Address = "м. Дніпро, просп. Слобожанський, 87", Phone = "+380 95 111 11 15", WorkSchedule = "08:00 - 21:00", Latitude = 48.5032, Longitude = 35.0715 },
                new Pharmacy { Name = "AIPharma №6 Калинова", Address = "м. Дніпро, вул. Калинова, 64", Phone = "+380 95 111 11 16", WorkSchedule = "08:00 - 20:00", Latitude = 48.5242, Longitude = 35.0485 },
                new Pharmacy { Name = "AIPharma №7 Робоча", Address = "м. Дніпро, вул. Робоча, 152", Phone = "+380 95 111 11 17", WorkSchedule = "08:00 - 21:00", Latitude = 48.4435, Longitude = 35.0025 },
                new Pharmacy { Name = "AIPharma №8 Парус", Address = "м. Дніпро, ж/м Парус, 12", Phone = "+380 95 111 11 18", WorkSchedule = "08:00 - 20:00", Latitude = 48.4748, Longitude = 34.9232 },
                new Pharmacy { Name = "AIPharma №9 Сонячний", Address = "м. Дніпро, ж/м Сонячний, 8", Phone = "+380 95 111 11 19", WorkSchedule = "08:00 - 21:00", Latitude = 48.4946, Longitude = 35.0901 },
                new Pharmacy { Name = "AIPharma №10 Лікарняна", Address = "м. Дніпро, вул. Ближня, 31", Phone = "+380 95 111 11 20", WorkSchedule = "Цілодобово", Latitude = 48.4561, Longitude = 35.0415 }
            );

            db.SaveChanges();
        }

        private static void SeedProducts(ApplicationDbContext db)
        {
            if (db.Products.Any()) return;

            var categories = db.ProductCategories.ToList();

            string[] commonNames =
            {
                "Парацетамол 500 мг", "Ібупрофен 200 мг", "Цитрамон", "Но-шпа", "Анальгін",
                "Аспірин Кардіо", "Нурофен", "Спазмалгон", "Панадол", "Диклофенак гель",
                "Вітамін C 500 мг", "Вітамін D3", "Магній B6", "Комплекс вітамінів A-Z", "Цинк актив",
                "Аква Маріс", "Називін", "Фарингосепт", "Стрепсілс", "Лазолван",
                "Амброксол", "Синупрет", "Евказолін", "Грипоцитрон", "Фервекс",
                "Активоване вугілля", "Смекта", "Регідрон", "Мезим", "Еспумізан",
                "Лоперамід", "Панкреатин", "Ренні", "Омепразол", "Лінекс",
                "Корвалол", "Валідол", "Каптоприл", "Бісопролол", "Кардіомагніл",
                "Перекис водню", "Хлоргексидин", "Йод", "Бинт стерильний", "Пластир медичний",
                "Маска медична", "Санітайзер", "Термометр електронний", "Глюкометр базовий", "Тест-смужки універсальні"
            };

            string[] uniqueNames =
            {
                "Дитячий сироп від кашлю BabyCough", "Вітаміни Kids Plus", "Спрей для горла HerbalMed", "Крем дерматологічний SkinCare", "Ортопедичний бинт Flex",
                "Тонометр автоматичний ProLife", "Інгалятор компресорний AirMed", "Пробіотик BioFlora", "Краплі очні VisionPlus", "Засіб для промивання носа SeaClean",
                "Гель для суглобів ArthroHelp", "Пластир зігріваючий HotPatch", "Кальцій D3 Forte", "Електроліти SportHydro", "Антисептичний спрей SafeSkin",
                "Шампунь лікувальний DermoPlus", "Крем дитячий BabySoft", "Сольовий розчин Nebula", "Чай трав'яний ImmunoTea", "Капсули Omega-3",
                "Фіточай для сну RelaxTea", "Сироп імунний ImmunoSyrup", "Спрей від алергії AllerStop", "Гель після опіків BurnCare", "Мазь загоювальна HealFast",
                "Вушні краплі OtoCare", "Бальзам від застуди ColdBalm", "Піна для вмивання PharmaClean", "Крем SPF 50 PharmaSun", "Гель алое VeraGel",
                "Дитячий термометр MiniTemp", "Іригатор портативний DentalJet", "Зубна паста SensitiveMed", "Ополіскувач OralFresh", "Пластир дитячий KidsPatch",
                "Пульсоксиметр OxyCheck", "Бандаж еластичний SupportBand", "Компрес холодовий IcePack", "Компрес тепловий WarmPack", "Набір першої допомоги",
                "Фіточай для травлення GastroTea", "Краплі для шлунку GastroDrop", "Спрей для ран WoundSafe", "Серветки спиртові AlcoholWipes", "Розчин для лінз ClearLens",
                "Крем для рук PharmaHand", "Гель для ніг VeinCool", "Вітаміни для волосся HairVit", "Вітаміни для зору EyeVit", "Мінеральний комплекс MineralMax"
            };

            var manufacturers = new[]
            {
                "Фармак", "Дарниця", "Борщагівський ХФЗ", "Київмедпрепарат", "Здоров'я",
                "Bayer", "Sandoz", "Teva", "KRKA", "PharmaLife"
            };

            var products = new List<Product>();
            var random = new Random(10);

            for (int i = 0; i < commonNames.Length; i++)
            {
                var category = categories[i % categories.Count];

                products.Add(new Product
                {
                    Name = commonNames[i],
                    ProductCategoryId = category.Id,
                    Manufacturer = manufacturers[i % manufacturers.Length],
                    Description = $"Товар «{commonNames[i]}» використовується згідно з інструкцією. Перед застосуванням рекомендовано ознайомитися з описом та за потреби звернутися до лікаря або фармацевта.",
                    Price = random.Next(35, 450),
                    DiscountPrice = i % 6 == 0 ? random.Next(25, 300) : null,
                    IsPrescriptionRequired = i % 11 == 0,
                    ImagePath = "/images/products/default.png"
                });
            }

            for (int i = 0; i < uniqueNames.Length; i++)
            {
                var category = categories[(i + 3) % categories.Count];

                products.Add(new Product
                {
                    Name = uniqueNames[i],
                    ProductCategoryId = category.Id,
                    Manufacturer = manufacturers[(i + 4) % manufacturers.Length],
                    Description = $"Унікальний товар «{uniqueNames[i]}», доступний лише в окремих аптеках мережі AIPharma. Наявність потрібно перевіряти перед оформленням замовлення.",
                    Price = random.Next(70, 1800),
                    DiscountPrice = i % 5 == 0 ? random.Next(60, 1200) : null,
                    IsPrescriptionRequired = i % 9 == 0,
                    ImagePath = "/images/products/default.png"
                });
            }

            db.Products.AddRange(products);
            db.SaveChanges();
        }

        private static void SeedStocks(ApplicationDbContext db)
        {
            if (db.ProductStocks.Any()) return;

            var products = db.Products.ToList();
            var pharmacies = db.Pharmacies.ToList();
            var random = new Random(15);

            var commonProductNames = db.Products
                .OrderBy(x => x.Id)
                .Take(50)
                .Select(x => x.Name)
                .ToHashSet();

            var stocks = new List<ProductStock>();

            foreach (var product in products)
            {
                if (commonProductNames.Contains(product.Name))
                {
                    foreach (var pharmacy in pharmacies)
                    {
                        stocks.Add(new ProductStock
                        {
                            ProductId = product.Id,
                            PharmacyId = pharmacy.Id,
                            Quantity = random.Next(15, 150)
                        });
                    }
                }
                else
                {
                    var selectedPharmacies = pharmacies
                        .OrderBy(x => random.Next())
                        .Take(random.Next(1, 4))
                        .ToList();

                    foreach (var pharmacy in selectedPharmacies)
                    {
                        stocks.Add(new ProductStock
                        {
                            ProductId = product.Id,
                            PharmacyId = pharmacy.Id,
                            Quantity = random.Next(2, 40)
                        });
                    }
                }
            }

            db.ProductStocks.AddRange(stocks);
            db.SaveChanges();
        }

        private static void SeedAiData(ApplicationDbContext db)
        {
            if (!db.SystemPrompts.Any())
            {
                db.SystemPrompts.Add(new SystemPrompt
                {
                    Name = "AIPharma Assistant",
                    IsActive = true,
                    PromptText =
                        "Ти інтелектуальний помічник веб-системи AIPharma. " +
                        "Ти допомагаєш користувачам знаходити товари, перевіряти наявність в аптеках, дізнаватися про знижки, графік роботи аптек і порядок оформлення замовлення. " +
                        "Ти не встановлюєш діагноз, не призначаєш лікування і не замінюєш консультацію лікаря або фармацевта. " +
                        "Якщо питання стосується лікування, дозування або небезпечних симптомів, порадь звернутися до лікаря."
                });

                db.SaveChanges();
            }

            if (db.FaqAnswers.Any()) return;

            db.FaqAnswers.AddRange(
                new FaqAnswer { Question = "Як оформити замовлення?", Answer = "Оберіть товар, відкрийте його сторінку, виберіть аптеку з наявністю та натисніть кнопку оформлення замовлення. Оплата здійснюється при отриманні.", Category = "Замовлення" },
                new FaqAnswer { Question = "Чи є онлайн оплата?", Answer = "Ні, онлайн-оплата в системі не передбачена. Оплатити замовлення можна при отриманні в аптеці готівкою або карткою.", Category = "Оплата" },
                new FaqAnswer { Question = "Як дізнатися чи є товар в аптеці?", Answer = "На сторінці товару відображається список аптек, у яких товар є в наявності, а також кількість на складі.", Category = "Наявність" },
                new FaqAnswer { Question = "Як додати товар в обране?", Answer = "На сторінці товару або в каталозі натисніть кнопку «В обране». Після цього товар буде доступний в особистому кабінеті.", Category = "Особистий кабінет" },
                new FaqAnswer { Question = "Як порівняти товари?", Answer = "Додайте кілька товарів до порівняння, після чого відкрийте сторінку порівняння в особистому кабінеті.", Category = "Порівняння" },
                new FaqAnswer { Question = "Де знаходяться аптеки?", Answer = "Список аптек і карта з позначками доступні на сторінці «Аптеки».", Category = "Аптеки" },
                new FaqAnswer { Question = "Чи можна замовити рецептурний препарат?", Answer = "Система може показувати наявність рецептурного товару, але відпуск таких препаратів здійснюється відповідно до чинних правил і за наявності рецепта.", Category = "Товари" },
                new FaqAnswer { Question = "Як переглянути товари зі знижкою?", Answer = "Перейдіть на сторінку «Знижки». Там відображаються товари, для яких вказана знижена ціна.", Category = "Знижки" },
                new FaqAnswer { Question = "Як залишити скаргу?", Answer = "У власному кабінеті або на сторінці аптеки можна створити звернення, обравши аптеку та описавши проблему.", Category = "Скарги" },
                new FaqAnswer { Question = "Чи може помічник призначити лікування?", Answer = "Ні. AI-помічник не призначає лікування і не замінює консультацію лікаря або фармацевта.", Category = "AI-помічник" }
            );

            db.SaveChanges();
        }

        private static void SeedEmployees(ApplicationDbContext db)
        {
            if (db.Employees.Any()) return;

            var pharmacies = db.Pharmacies.ToList();
            var random = new Random(25);
            var employees = new List<Employee>();

            foreach (var pharmacy in pharmacies)
            {
                employees.Add(new Employee
                {
                    PharmacyId = pharmacy.Id,
                    FullName = $"Фармацевт аптеки {pharmacy.Id}",
                    Position = "Фармацевт",
                    HourlyRate = random.Next(95, 150)
                });

                employees.Add(new Employee
                {
                    PharmacyId = pharmacy.Id,
                    FullName = $"Адміністратор аптеки {pharmacy.Id}",
                    Position = "Адміністратор",
                    HourlyRate = random.Next(120, 180)
                });
            }

            db.Employees.AddRange(employees);
            db.SaveChanges();

            var savedEmployees = db.Employees.ToList();
            var workLogs = new List<EmployeeWorkLog>();

            foreach (var employee in savedEmployees)
            {
                for (int i = 0; i < 7; i++)
                {
                    workLogs.Add(new EmployeeWorkLog
                    {
                        EmployeeId = employee.Id,
                        WorkDate = DateTime.Today.AddDays(-i),
                        HoursWorked = random.Next(6, 10)
                    });
                }
            }

            db.EmployeeWorkLogs.AddRange(workLogs);

            db.EmployeeVacations.Add(new EmployeeVacation
            {
                EmployeeId = savedEmployees.First().Id,
                StartDate = DateTime.Today.AddDays(10),
                EndDate = DateTime.Today.AddDays(17),
                Status = "Заплановано"
            });

            db.EmployeeSickLeaves.Add(new EmployeeSickLeave
            {
                EmployeeId = savedEmployees.Last().Id,
                StartDate = DateTime.Today.AddDays(-3),
                EndDate = DateTime.Today.AddDays(-1),
                Comment = "Тимчасова непрацездатність"
            });

            db.SaveChanges();
        }
    }
}