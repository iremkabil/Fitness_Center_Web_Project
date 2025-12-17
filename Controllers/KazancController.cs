using Fitness_Center_Web_Project.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Fitness_Center_Web_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KazancController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KazancController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        // GET: /api/Kazanc/AylikKazanclar?year=2025
        [HttpGet("AylikKazanclar")]
        public async Task<IActionResult> GetAylikKazanclar([FromQuery] int? year = null)
        {
            if (!IsAdmin())
                return Unauthorized(new { message = "Admin girişi gerekli." });

            int y = year ?? DateTime.Now.Year;
            var culture = new CultureInfo("tr-TR");

            var sonuc = new List<object>();

            for (int ay = 1; ay <= 12; ay++)
            {
                var baslangic = new DateTime(y, ay, 1);
                var bitis = baslangic.AddMonths(1); // [baslangic, bitis)

                var toplam = await _context.Randevular
                    .Where(r => r.Durum == "Onaylandı"
                                && r.RandevuTarihi >= baslangic
                                && r.RandevuTarihi < bitis)
                    .SumAsync(r => (decimal?)r.Ucret) ?? 0m;

                sonuc.Add(new
                {
                    AyNo = ay,
                    Ay = culture.DateTimeFormat.GetMonthName(ay),
                    Kazanc = toplam
                });
            }

            return Ok(sonuc);
        }
    }
}
// API endpoint’i gelecek yıl yerine bu yıl (istersen query ile yıl seçilebilir)
// Admin kontrolü API için Redirect değil, Unauthorized/Forbid döner
// Ay isimleri Türkçe görünsün diye tr-TR culture kullanır
// Boş aylar 0 döner