using AIPharma.Data;
using AIPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        private IActionResult Deny()
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var model = await BuildDashboardModel();

            return View(model);
        }

        public async Task<IActionResult> Statistics()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var model = await BuildDashboardModel();

            return View(model);
        }

        public async Task<IActionResult> Orders()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Pharmacy)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Pharmacy)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            await _db.SaveChangesAsync();

            return RedirectToAction("OrderDetails", new { id });
        }

        public async Task<IActionResult> Complaints()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var complaints = await _db.Complaints
                .Include(c => c.User)
                .Include(c => c.Pharmacy)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

        public async Task<IActionResult> ComplaintDetails(int id)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var complaint = await _db.Complaints
                .Include(c => c.User)
                .Include(c => c.Pharmacy)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateComplaintStatus(int id, string status)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var complaint = await _db.Complaints.FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
            {
                return NotFound();
            }

            complaint.Status = status;
            await _db.SaveChangesAsync();

            return RedirectToAction("ComplaintDetails", new { id });
        }

        public async Task<IActionResult> Products(string? search)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var query = _db.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductStocks)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Manufacturer.Contains(search) ||
                    p.Description.Contains(search));
            }

            ViewBag.Search = search;

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            ViewBag.Categories = await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            string name,
            int productCategoryId,
            string manufacturer,
            string description,
            decimal price,
            decimal? discountPrice,
            bool isPrescriptionRequired)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            if (string.IsNullOrWhiteSpace(name) || productCategoryId <= 0 || price <= 0)
            {
                ViewBag.Error = "Назва, категорія та ціна є обов'язковими.";
                ViewBag.Categories = await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync();
                return View();
            }

            var product = new Product
            {
                Name = name,
                ProductCategoryId = productCategoryId,
                Manufacturer = manufacturer ?? "",
                Description = description ?? "",
                Price = price,
                DiscountPrice = discountPrice,
                IsPrescriptionRequired = isPrescriptionRequired,
                ImagePath = "/images/products/default.png",
                CreatedAt = DateTime.Now
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            var pharmacies = await _db.Pharmacies.ToListAsync();

            foreach (var pharmacy in pharmacies)
            {
                _db.ProductStocks.Add(new ProductStock
                {
                    ProductId = product.Id,
                    PharmacyId = pharmacy.Id,
                    Quantity = 0
                });
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var product = await _db.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(
            int id,
            string name,
            int productCategoryId,
            string manufacturer,
            string description,
            decimal price,
            decimal? discountPrice,
            bool isPrescriptionRequired)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(name) || productCategoryId <= 0 || price <= 0)
            {
                ViewBag.Error = "Назва, категорія та ціна є обов'язковими.";
                ViewBag.Categories = await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync();
                return View(product);
            }

            product.Name = name;
            product.ProductCategoryId = productCategoryId;
            product.Manufacturer = manufacturer ?? "";
            product.Description = description ?? "";
            product.Price = price;
            product.DiscountPrice = discountPrice;
            product.IsPrescriptionRequired = isPrescriptionRequired;

            await _db.SaveChangesAsync();

            return RedirectToAction("Products");
        }

        public async Task<IActionResult> Stocks(int? productId, int? pharmacyId)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var query = _db.ProductStocks
                .Include(s => s.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .Include(s => s.Pharmacy)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(s => s.ProductId == productId.Value);
            }

            if (pharmacyId.HasValue)
            {
                query = query.Where(s => s.PharmacyId == pharmacyId.Value);
            }

            ViewBag.Products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Pharmacies = await _db.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            ViewBag.ProductId = productId;
            ViewBag.PharmacyId = pharmacyId;

            var stocks = await query
                .OrderBy(s => s.Product!.Name)
                .ThenBy(s => s.Pharmacy!.Name)
                .ToListAsync();

            return View(stocks);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int stockId, int quantity)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            if (quantity < 0)
            {
                quantity = 0;
            }

            var stock = await _db.ProductStocks.FirstOrDefaultAsync(s => s.Id == stockId);

            if (stock == null)
            {
                return NotFound();
            }

            stock.Quantity = quantity;
            await _db.SaveChangesAsync();

            return RedirectToAction("Stocks");
        }

        public async Task<IActionResult> Faq()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var faq = await _db.FaqAnswers
                .OrderByDescending(f => f.UseCount)
                .ThenBy(f => f.Category)
                .ToListAsync();

            return View(faq);
        }

        [HttpGet]
        public IActionResult CreateFaq()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateFaq(string question, string answer, string category)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
            {
                ViewBag.Error = "Питання та відповідь є обов'язковими.";
                return View();
            }

            _db.FaqAnswers.Add(new FaqAnswer
            {
                Question = question,
                Answer = answer,
                Category = string.IsNullOrWhiteSpace(category) ? "Загальні питання" : category,
                UseCount = 0,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            return RedirectToAction("Faq");
        }

        [HttpGet]
        public async Task<IActionResult> EditFaq(int id)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var faq = await _db.FaqAnswers.FirstOrDefaultAsync(f => f.Id == id);

            if (faq == null)
            {
                return NotFound();
            }

            return View(faq);
        }

        [HttpPost]
        public async Task<IActionResult> EditFaq(int id, string question, string answer, string category)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var faq = await _db.FaqAnswers.FirstOrDefaultAsync(f => f.Id == id);

            if (faq == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
            {
                ViewBag.Error = "Питання та відповідь є обов'язковими.";
                return View(faq);
            }

            faq.Question = question;
            faq.Answer = answer;
            faq.Category = string.IsNullOrWhiteSpace(category) ? "Загальні питання" : category;

            await _db.SaveChangesAsync();

            return RedirectToAction("Faq");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFaq(int id)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var faq = await _db.FaqAnswers.FirstOrDefaultAsync(f => f.Id == id);

            if (faq != null)
            {
                _db.FaqAnswers.Remove(faq);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Faq");
        }
        public async Task<IActionResult> AiDialogs()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var sessions = await _db.ChatSessions
                .Include(s => s.User)
                .Include(s => s.Messages)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(sessions);
        }

        public async Task<IActionResult> AiDialogDetails(int id)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var session = await _db.ChatSessions
                .Include(s => s.User)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            session.Messages = session.Messages
                .OrderBy(m => m.CreatedAt)
                .ToList();

            return View(session);
        }

        public async Task<IActionResult> AiCache()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var cachedAnswers = await _db.AiCachedAnswers
                .OrderByDescending(c => c.LastUsedAt)
                .ToListAsync();

            return View(cachedAnswers);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCachedAnswer(int id)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var cachedAnswer = await _db.AiCachedAnswers.FirstOrDefaultAsync(c => c.Id == id);

            if (cachedAnswer != null)
            {
                _db.AiCachedAnswers.Remove(cachedAnswer);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("AiCache");
        }

        [HttpPost]
        public async Task<IActionResult> ClearAiCache()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var cachedAnswers = await _db.AiCachedAnswers.ToListAsync();

            _db.AiCachedAnswers.RemoveRange(cachedAnswers);
            await _db.SaveChangesAsync();

            return RedirectToAction("AiCache");
        }

        public async Task<IActionResult> ApiLogs()
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var logs = await _db.ApiRequestLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Take(200)
                .ToListAsync();

            return View(logs);
        }

        public async Task<IActionResult> ApiLogDetails(int id)
        {
            if (!IsAdmin())
            {
                return Deny();
            }

            var log = await _db.ApiRequestLogs
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        private async Task<AdminDashboardViewModel> BuildDashboardModel()
        {
            var model = new AdminDashboardViewModel
            {
                UsersCount = await _db.Users.CountAsync(),
                ProductsCount = await _db.Products.CountAsync(),
                PharmaciesCount = await _db.Pharmacies.CountAsync(),
                OrdersCount = await _db.Orders.CountAsync(),
                ComplaintsCount = await _db.Complaints.CountAsync(),
                FaqCount = await _db.FaqAnswers.CountAsync(),
                ChatMessagesCount = await _db.ChatMessages.CountAsync()
            };

            model.OrdersByPharmacy = await _db.Pharmacies
                .Select(p => new AdminPharmacyOrderStat
                {
                    PharmacyName = p.Name,
                    OrdersCount = p.Orders.Count,
                    TotalAmount = p.Orders.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.OrdersCount)
                .ToListAsync();

            model.PopularProducts = await _db.OrderItems
                .Include(i => i.Product)
                .GroupBy(i => i.Product!.Name)
                .Select(g => new AdminPopularProductStat
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalAmount = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToListAsync();

            model.ComplaintsByPharmacy = await _db.Pharmacies
                .Select(p => new AdminComplaintStat
                {
                    PharmacyName = p.Name,
                    ComplaintsCount = p.Complaints.Count
                })
                .OrderByDescending(x => x.ComplaintsCount)
                .ToListAsync();

            model.AiStats = await _db.ApiRequestLogs
                .GroupBy(x => x.Source)
                .Select(g => new AdminAiStat
                {
                    Source = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            model.EmployeeSalaryStats = await _db.Employees
                .Include(e => e.Pharmacy)
                .Include(e => e.WorkLogs)
                .Select(e => new AdminEmployeeSalaryStat
                {
                    EmployeeName = e.FullName,
                    PharmacyName = e.Pharmacy != null ? e.Pharmacy.Name : "",
                    Position = e.Position,
                    HourlyRate = e.HourlyRate,
                    HoursWorked = e.WorkLogs.Sum(w => w.HoursWorked),
                    Salary = e.WorkLogs.Sum(w => w.HoursWorked) * e.HourlyRate
                })
                .OrderByDescending(x => x.Salary)
                .ToListAsync();

            return model;
        }
    }
}