using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Center_Web_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Admin sabit bilgileri (ödev gereği)
        private const string AdminEmail = "G221210027@sakarya.edu.tr";
        private const string AdminPassword = "sau";

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserLogin model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Admin kontrolü (DB'den bağımsız)
            if (string.Equals(model.Email, AdminEmail, StringComparison.OrdinalIgnoreCase) &&
                model.Sifre == AdminPassword)
            {
                HttpContext.Session.SetString("Username", "Admin");
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("Email", AdminEmail);
                return RedirectToAction("AdminDashboard", "Admin");
            }

            var user = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == model.Email && u.Sifre == model.Sifre);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Geçersiz e-posta veya şifre!";
                return View(model);
            }

            HttpContext.Session.SetString("Username", $"{user.Ad} {user.Soyad}");
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Role", user.Role ?? "User");
            HttpContext.Session.SetString("Email", user.Email ?? "");

            return RedirectToAction("UserDashboard", "User");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Fitness_Center_Web_Project.Models.User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            bool emailExists = _context.Users.Any(u => u.Email == user.Email);
            if (emailExists)
            {
                // Buradaki kritik düzeltme: User çakışmasın diye full name kullandık
                ModelState.AddModelError(
                    nameof(Fitness_Center_Web_Project.Models.User.Email),
                    "Bu e-posta zaten kayıtlı."
                );
                return View(user);
            }

            // Güvenlik: formdan Role/Admin gelirse bile ez
            user.Role = "User";

            _context.Users.Add(user);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ViewBag.ErrorMessage = "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyiniz.";
                return View(user);
            }

            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
