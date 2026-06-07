using AIPharma.Data;
using AIPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Заповніть усі поля.";
                return View();
            }

            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                ViewBag.Error = "Користувач з такою електронною поштою вже існує.";
                return View();
            }

            var userRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            if (userRole == null)
            {
                ViewBag.Error = "Роль користувача не знайдена в базі даних.";
                return View();
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = userRole.Id,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", "User");

            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Введіть електронну пошту та пароль.";
                return View();
            }

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Невірна електронна пошта або пароль.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role?.Name ?? "User");

            if (user.Role?.Name == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Profile");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}