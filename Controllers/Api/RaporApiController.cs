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

        // GET: /api/RaporApi/AylikKazanc?year=2025
        [HttpGet("AylikKazanc")]
        public async Task<IActionResult> AylikKazanc([FromQuery] int? year)
        {
            int yil = year ?? DateTime.Now.Year;

            // Parantez önemli: Year filtresi her iki durum için de geçerli olmalı
            var raw = await _context.Randevular
                .AsNoTracking()
                .Where(r =>
                    r.RandevuTarihi.Year == yil &&
                    (r.Durum == "Onaylandı" || r.Durum == "Tamamlandı"))
                .GroupBy(r => r.RandevuTarihi.Month)
                .Select(g => new
                {
                    AyNo = g.Key,
                    Kazanc = g.Sum(x => x.Ucret)
                })
                .OrderBy(x => x.AyNo)
                .ToListAsync();

            // String formatı burada (client side)
            var list = raw.Select(x => new KazancDto
            {
                Ay = $"{yil}-{x.AyNo:00}",
                Kazanc = x.Kazanc
            }).ToList();

            return Ok(list);
        }

        public class KazancDto
        {
            public string Ay { get; set; } = "";
            public decimal Kazanc { get; set; }
        }

    }
}
