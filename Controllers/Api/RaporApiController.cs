using Fitness_Center_Web_Project.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Center_Web_Project.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")] // => /api/RaporApi
    public class RaporApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RaporApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/RaporApi/aylik-kazanc?year=2025
        [HttpGet("aylik-kazanc")]
        public async Task<IActionResult> AylikKazanc([FromQuery] int? year)
        {
            int yil = year ?? DateTime.Now.Year;

            var list = await _context.Randevular
                .AsNoTracking()
                .Where(r =>
                    r.RandevuTarihi.Year == yil &&
                    (r.Durum == "Onaylandı" || r.Durum == "Tamamlandı"))
                .GroupBy(r => r.RandevuTarihi.Month)
                .Select(g => new
                {
                    Ay = yil.ToString() + "-" + (g.Key < 10 ? "0" : "") + g.Key.ToString(),
                    Kazanc = g.Sum(x => x.Ucret)
                })
                .OrderBy(x => x.Ay)
                .ToListAsync();

            return Ok(list);
        }

        public class KazancDto
        {
            public string Ay { get; set; } = "";
            public decimal Kazanc { get; set; }
        }

    }
}
