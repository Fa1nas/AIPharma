using AIPharma.Data;
using AIPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ManagerController(ApplicationDbContext db)
        {
            _db = db;
        }

        private bool IsManager()
        {
            return HttpContext.Session.GetString("UserRole") == "Manager";
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        private IActionResult Deny()
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        private async Task<int?> GetManagerPharmacyId()
        {
            if (CurrentUserId == null)
            {
                return null;
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId.Value);
            return user?.PharmacyId;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsManager())
            {
                return Deny();
            }

            var pharmacyId = await GetManagerPharmacyId();

            if (pharmacyId == null)
            {
                return Deny();
            }

            var pharmacy = await _db.Pharmacies.FirstOrDefaultAsync(p => p.Id == pharmacyId.Value);

            if (pharmacy == null)
            {
                return NotFound();
            }

            var model = new ManagerDashboardViewModel
            {
                PharmacyName = pharmacy.Name,
                PharmacyAddress = pharmacy.Address,
                OrdersCount = await _db.Orders.CountAsync(o => o.PharmacyId == pharmacyId.Value),
                ComplaintsCount = await _db.Complaints.CountAsync(c => c.PharmacyId == pharmacyId.Value),
                ProductsInStockCount = await _db.ProductStocks.CountAsync(s => s.PharmacyId == pharmacyId.Value && s.Quantity > 0),
                EmployeesCount = await _db.Employees.CountAsync(e => e.PharmacyId == pharmacyId.Value),
                TotalOrdersAmount = await _db.Orders
                    .Where(o => o.PharmacyId == pharmacyId.Value)
                    .SumAsync(o => o.TotalAmount)
            };

            model.PopularProducts = await _db.OrderItems
                .Include(i => i.Order)
                .Include(i => i.Product)
                .Where(i => i.Order != null && i.Order.PharmacyId == pharmacyId.Value)
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

            model.EmployeeSalaryStats = await _db.Employees
                .Include(e => e.Pharmacy)
                .Include(e => e.WorkLogs)
                .Where(e => e.PharmacyId == pharmacyId.Value)
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

            return View(model);
        }

        public async Task<IActionResult> Stocks()
        {
            if (!IsManager())
            {
                return Deny();
            }

            var pharmacyId = await GetManagerPharmacyId();

            if (pharmacyId == null)
            {
                return Deny();
            }

            var stocks = await _db.ProductStocks
                .Include(s => s.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .Include(s => s.Pharmacy)
                .Where(s => s.PharmacyId == pharmacyId.Value)
                .OrderBy(s => s.Product!.Name)
                .ToListAsync();

            return View(stocks);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int stockId, int quantity)
        {
            if (!IsManager())
            {
                return Deny();
            }

            var pharmacyId = await GetManagerPharmacyId();

            if (pharmacyId == null)
            {
                return Deny();
            }

            if (quantity < 0)
            {
                quantity = 0;
            }

            var stock = await _db.ProductStocks
                .FirstOrDefaultAsync(s => s.Id == stockId && s.PharmacyId == pharmacyId.Value);

            if (stock == null)
            {
                return NotFound();
            }

            stock.Quantity = quantity;
            await _db.SaveChangesAsync();

            return RedirectToAction("Stocks");
        }

        public async Task<IActionResult> Employees()
        {
            if (!IsManager())
            {
                return Deny();
            }

            var pharmacyId = await GetManagerPharmacyId();

            if (pharmacyId == null)
            {
                return Deny();
            }

            var employees = await _db.Employees
                .Include(e => e.Pharmacy)
                .Include(e => e.WorkLogs)
                .Where(e => e.PharmacyId == pharmacyId.Value)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            return View(employees);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployeeRate(int employeeId, decimal hourlyRate)
        {
            if (!IsManager())
            {
                return Deny();
            }

            var pharmacyId = await GetManagerPharmacyId();

            if (pharmacyId == null)
            {
                return Deny();
            }

            if (hourlyRate < 0)
            {
                hourlyRate = 0;
            }

            var employee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.PharmacyId == pharmacyId.Value);

            if (employee == null)
            {
                return NotFound();
            }

            employee.HourlyRate = hourlyRate;
            await _db.SaveChangesAsync();

            return RedirectToAction("Employees");
        }

        public async Task<IActionResult> Complaints()
        {
            if (!IsManager())
            {
                return Deny();
            }

            var pharmacyId = await GetManagerPharmacyId();

            if (pharmacyId == null)
            {
                return Deny();
            }

            var complaints = await _db.Complaints
                .Include(c => c.User)
                .Include(c => c.Pharmacy)
                .Where(c => c.PharmacyId == pharmacyId.Value)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

        public async Task<IActionResult> ComplaintDetails(int id)
        {
            if (!IsManager())
            {
                return Deny();
            }

            var pharmacyId = await GetManagerPharmacyId();

            if (pharmacyId == null)
            {
                return Deny();
            }

            var complaint = await _db.Complaints
                .Include(c => c.User)
                .Include(c => c.Pharmacy)
                .FirstOrDefaultAsync(c => c.Id == id && c.PharmacyId == pharmacyId.Value);

            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateComplaintStatus(int id, string status)
        {
            if (!IsManager())
            {
                return Deny();
            }

            var pharmacyId = await GetManagerPharmacyId();

            if (pharmacyId == null)
            {
                return Deny();
            }

            var complaint = await _db.Complaints
                .FirstOrDefaultAsync(c => c.Id == id && c.PharmacyId == pharmacyId.Value);

            if (complaint == null)
            {
                return NotFound();
            }

            complaint.Status = status;
            await _db.SaveChangesAsync();

            return RedirectToAction("ComplaintDetails", new { id });
        }
    }
}