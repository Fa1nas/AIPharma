using AIPharma.Data;
using AIPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProfileController(ApplicationDbContext db)
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

            var user = await _db.Users
                .Include(u => u.Role)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Pharmacy)
                .Include(u => u.Favorites)
                .Include(u => u.ChatSessions)
                .FirstOrDefaultAsync(u => u.Id == CurrentUserId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToLogin();
            }

            return View(user);
        }

        public async Task<IActionResult> Favorites()
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var favorites = await _db.Favorites
                .Include(f => f.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .Include(f => f.Product)
                    .ThenInclude(p => p!.ProductStocks)
                .Where(f => f.UserId == CurrentUserId.Value)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(favorites);
        }

        public async Task<IActionResult> Compare()
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var comparisons = await _db.ProductComparisons
                .Include(c => c.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .Include(c => c.Product)
                    .ThenInclude(p => p!.ProductStocks)
                .Where(c => c.UserId == CurrentUserId.Value)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(comparisons);
        }

        public async Task<IActionResult> AddFavorite(int productId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var exists = await _db.Favorites.AnyAsync(f =>
                f.UserId == CurrentUserId.Value &&
                f.ProductId == productId);

            if (!exists)
            {
                _db.Favorites.Add(new Favorite
                {
                    UserId = CurrentUserId.Value,
                    ProductId = productId
                });

                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Products", new { id = productId });
        }

        public async Task<IActionResult> RemoveFavorite(int productId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var favorite = await _db.Favorites.FirstOrDefaultAsync(f =>
                f.UserId == CurrentUserId.Value &&
                f.ProductId == productId);

            if (favorite != null)
            {
                _db.Favorites.Remove(favorite);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Favorites");
        }

        public async Task<IActionResult> AddCompare(int productId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var exists = await _db.ProductComparisons.AnyAsync(c =>
                c.UserId == CurrentUserId.Value &&
                c.ProductId == productId);

            if (!exists)
            {
                _db.ProductComparisons.Add(new ProductComparison
                {
                    UserId = CurrentUserId.Value,
                    ProductId = productId
                });

                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Products", new { id = productId });
        }

        public async Task<IActionResult> RemoveCompare(int productId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            var comparison = await _db.ProductComparisons.FirstOrDefaultAsync(c =>
                c.UserId == CurrentUserId.Value &&
                c.ProductId == productId);

            if (comparison != null)
            {
                _db.ProductComparisons.Remove(comparison);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Compare");
        }
    }
}