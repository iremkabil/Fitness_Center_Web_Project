using Fitness_Center_Web_Project.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Center_Web_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult UserDashboard()
        {
            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");

            // Giriş yoksa veya Admin ise user paneline sokma
            if (string.IsNullOrWhiteSpace(role) || role == "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = username ?? "Kullanıcı";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Randevularim()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrWhiteSpace(role) || role == "Admin")
                return RedirectToAction("Login", "Account");

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Account");

            var randevular = await _context.Randevular
                .AsNoTracking()
                .Include(r => r.Personel)
                .Include(r => r.Islem)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RandevuTarihi)
                .ThenByDescending(r => r.RandevuSaati)
                .ToListAsync();

            return View(randevular);
        }
    }
}
