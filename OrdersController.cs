using AIPharma.Data;
using AIPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var orders = await _db.Orders
                .Include(o => o.Pharmacy)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == CurrentUserId.Value)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Pharmacy)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == CurrentUserId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int productId, int pharmacyId, int quantity)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            if (quantity <= 0)
            {
                TempData["OrderError"] = "Кількість товару повинна бути більшою за нуль.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var stock = await _db.ProductStocks
                .Include(s => s.Product)
                .Include(s => s.Pharmacy)
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.PharmacyId == pharmacyId);

            if (stock == null || stock.Product == null || stock.Pharmacy == null)
            {
                TempData["OrderError"] = "Товар не знайдено в обраній аптеці.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            if (stock.Quantity < quantity)
            {
                TempData["OrderError"] = "Недостатня кількість товару на складі обраної аптеки.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var unitPrice = stock.Product.DiscountPrice ?? stock.Product.Price;
            var totalAmount = unitPrice * quantity;

            var order = new Order
            {
                UserId = CurrentUserId.Value,
                PharmacyId = pharmacyId,
                Status = "Новий",
                PaymentType = "Оплата при отриманні",
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = unitPrice
                    }
                }
            };

            stock.Quantity -= quantity;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = order.Id });
        }
    }
}