using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fitness_Center_Web_Project.Context;
using System.Net.Http.Headers;

namespace Fitness_Center_Web_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult UserDashboard()
        {
            // Session'dan rolü al
            var role = HttpContext.Session.GetString("Role");
			var username = HttpContext.Session.GetString("Username");



			// Eğer kullanıcı adminse, erişim reddedilir
			if (string.IsNullOrEmpty(role) || role == "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
			ViewBag.UserName = username;
			
			return View();
        }

        public async Task<IActionResult> Randevularim()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var randevular = await _context.Randevular
                .Include(r => r.Personel)
                .Include(r => r.Islem)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return View(randevular);
        }

    }
}
