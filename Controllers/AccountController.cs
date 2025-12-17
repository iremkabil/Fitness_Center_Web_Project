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

        // Giriş sayfası
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Giriş işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserLogin model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Admin kontrolü (DB'ye bağımlı olmasın diye önce kontrol)
            if (string.Equals(model.Email, AdminEmail, StringComparison.OrdinalIgnoreCase)
                && model.Sifre == AdminPassword)
            {
                HttpContext.Session.SetString("Username", "Admin");
                HttpContext.Session.SetString("Role", "Admin");
                return RedirectToAction("AdminDashboard", "Admin");
            }

            var user = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == model.Email && u.Sifre == model.Sifre);

            if (user is null)
            {
                ViewBag.ErrorMessage = "Geçersiz e-posta veya şifre!";
                return View(model);
            }

            HttpContext.Session.SetString("Username", $"{user.Ad} {user.Soyad}");
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Role", user.Role);

            // UserController yoksa Home/Index'e yönlendirebilirsin.
            return RedirectToAction("UserDashboard", "User");
        }

        // Kayıt sayfası
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Kayıt işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            // Aynı email ile ikinci kayıt olmasın
            bool emailExists = _context.Users.Any(u => u.Email == user.Email);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(User.Email), "Bu e-posta zaten kayıtlı.");
                return View(user);
            }

            // Rolü güvenli set et (formdan Admin gelmesini engeller)
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

        // Çıkış işlemi
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
