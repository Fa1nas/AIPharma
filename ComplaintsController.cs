using AIPharma.Data;
using AIPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ComplaintsController(ApplicationDbContext db)
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

            var complaints = await _db.Complaints
                .Include(c => c.Pharmacy)
                .Where(c => c.UserId == CurrentUserId.Value)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var complaint = await _db.Complaints
                .Include(c => c.Pharmacy)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId.Value);

            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? pharmacyId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            ViewBag.Pharmacies = await _db.Pharmacies
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.SelectedPharmacyId = pharmacyId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int pharmacyId, string title, string text)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            if (pharmacyId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(text))
            {
                ViewBag.Error = "Оберіть аптеку, вкажіть тему та текст звернення.";
                ViewBag.Pharmacies = await _db.Pharmacies.OrderBy(p => p.Name).ToListAsync();
                ViewBag.SelectedPharmacyId = pharmacyId;
                return View();
            }

            var pharmacyExists = await _db.Pharmacies.AnyAsync(p => p.Id == pharmacyId);

            if (!pharmacyExists)
            {
                ViewBag.Error = "Обрану аптеку не знайдено.";
                ViewBag.Pharmacies = await _db.Pharmacies.OrderBy(p => p.Name).ToListAsync();
                ViewBag.SelectedPharmacyId = pharmacyId;
                return View();
            }

            var complaint = new Complaint
            {
                UserId = CurrentUserId.Value,
                PharmacyId = pharmacyId,
                Title = title,
                Text = text,
                Status = "Нова",
                CreatedAt = DateTime.Now
            };

            _db.Complaints.Add(complaint);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = complaint.Id });
        }
    }
}