using AIPharma.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _db.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductStocks)
                    .ThenInclude(s => s.Pharmacy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Manufacturer.Contains(search) ||
                    p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.ProductCategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => (p.DiscountPrice ?? p.Price) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => (p.DiscountPrice ?? p.Price) <= maxPrice.Value);
            }

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Categories = await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync();

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }
        public async Task<IActionResult> Discounts(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _db.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductStocks)
                    .ThenInclude(s => s.Pharmacy)
                .Where(p => p.DiscountPrice != null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Manufacturer.Contains(search) ||
                    p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.ProductCategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => (p.DiscountPrice ?? p.Price) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => (p.DiscountPrice ?? p.Price) <= maxPrice.Value);
            }

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Categories = await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync();

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductStocks)
                    .ThenInclude(s => s.Pharmacy)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}