using Fitness_Center_Web_Project.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Center_Web_Project.Controllers.Api
{
    [ApiController]
    [Route("api/PersonelApi")]
    public class PersonelApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PersonelApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/personel/uygun?islemId=1&tarih=2025-12-22&saat=12:00
        // LINQ filtreleme + uygunluk kontrol (mesai + çakışma)
        [HttpGet("uygun")]
        public async Task<IActionResult> UygunPersoneller([FromQuery] int islemId, [FromQuery] DateTime tarih, [FromQuery] TimeSpan saat)
        {
            if (islemId <= 0) return BadRequest("islemId zorunlu.");

            var islem = await _context.Islemler.AsNoTracking().FirstOrDefaultAsync(i => i.Id == islemId);
            if (islem == null) return BadRequest("İşlem bulunamadı.");

            var baslangic = tarih.Date.Add(saat);
            var bitis = baslangic.Add(islem.Sure);
            var gun = baslangic.DayOfWeek;

            // 1) İşleme uygun personeller (LINQ filtreleme) + mesai include
            var adaylar = await _context.Personeller
                .AsNoTracking()
                .Include(p => p.Uzmanliklar)
                .Include(p => p.Mesailer).ThenInclude(m => m.CalistigiGunler)
                .Where(p => p.AktifMi)
                .Where(p => p.Uzmanliklar.Any(u => u.Islemler.Any(i => i.Id == islemId)))   // LINQ filtreleme
                .ToListAsync();

            // 2) Mesai uygunluğu (C# tarafı)
            var mesaiUygun = adaylar.Where(p =>
            {
                var m = p.Mesailer?.FirstOrDefault();
                if (m == null) return false;

                bool gunUygun = m.CalistigiGunler != null && m.CalistigiGunler.Any(g => g.Gun == gun);
                if (!gunUygun) return false;

                var mesaiBas = baslangic.Date.Add(m.BaslangicZamani);
                var mesaiBit = baslangic.Date.Add(m.BitisZamani);

                return !(baslangic < mesaiBas || bitis > mesaiBit);
            }).ToList();

            // 3) Çakışma kontrolü için o günkü randevuları çek
            var personelIds = mesaiUygun.Select(p => p.Id).ToList();

            var gunRandevulari = await _context.Randevular
                .AsNoTracking()
                .Where(r => personelIds.Contains(r.PersonelId)
                            && r.RandevuTarihi.Date == tarih.Date
                            && (r.Durum == "Beklemede" || r.Durum == "Onaylandı"))
                .ToListAsync();

            // 4) Çakışmayanları dön
            var uygun = mesaiUygun
                .Where(p =>
                {
                    var prsR = gunRandevulari.Where(r => r.PersonelId == p.Id);
                    return !prsR.Any(r =>
                    {
                        var rBas = r.RandevuTarihi.Date.Add(r.RandevuSaati);
                        var rBit = rBas.Add(r.Sure);
                        return rBas < bitis && rBit > baslangic;
                    });
                })
                .Select(p => new { p.Id, AdSoyad = $"{p.Ad} {p.Soyad}" })
                .ToList();

            return Ok(uygun);
        }

        // GET: /api/personel/tumu  -> “Tüm antrenörleri listeleme” örneği
        [HttpGet("tumu")]
        public async Task<IActionResult> Tumu()
        {
            var list = await _context.Personeller
                .AsNoTracking()
                .Where(p => p.AktifMi)     // LINQ filtreleme
                .Select(p => new { p.Id, p.Ad, p.Soyad, p.Cinsiyet })
                .ToListAsync();

            return Ok(list);
        }
    }
}
