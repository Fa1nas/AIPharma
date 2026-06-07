using AIPharma.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class PharmaciesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PharmaciesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var pharmacies = await _db.Pharmacies
                .Include(p => p.ProductStocks)
                .Include(p => p.Orders)
                .Include(p => p.Complaints)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(pharmacies);
        }
    }
}