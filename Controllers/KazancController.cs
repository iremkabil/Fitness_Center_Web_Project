using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fitness_Center_Web_Project.Context;

namespace Fitness_Center_Web_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KazancController : ControllerBase
    {
        private readonly AppDbContext _context;
        private IActionResult CheckAdminRole()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin")
            {
                return null; // Admin ise herhangi bir işlem yapmadan devam et
            }
            return RedirectToAction("UserDashboard", "User"); // Kullanıcı paneline yönlendir
        }
        public KazancController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("AylikKazanclar")]
        public async Task<ActionResult<IEnumerable<object>>> GetAylikKazanclar()
        {
            
            var sonuc = new List<object>();
            var simdikiTarih = DateTime.Now;
            var gelecekYil = simdikiTarih.Year + 1;

            for (int ay = 1; ay <= 12; ay++)
            {
                var baslangicTarihi = new DateTime(gelecekYil, ay, 1);
                var bitisTarihi = baslangicTarihi.AddMonths(1).AddDays(-1);

                var toplamKazanc = await _context.Randevular
                    .Where(r => r.Durum == "Onaylandı" && r.RandevuTarihi >= baslangicTarihi && r.RandevuTarihi <= bitisTarihi)
                    .SumAsync(r => r.Ucret);

                sonuc.Add(new
                {
                    Ay = baslangicTarihi.ToString("MMMM"), // Ayın ismini alır
                    Kazanc = toplamKazanc
                });
            }

            return Ok(sonuc);
        }
    }
}
