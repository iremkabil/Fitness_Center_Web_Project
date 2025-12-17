using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;

namespace Fitness_Center_Web_Project.Controllers
{
    public class RandevuController : Controller
    {
        private readonly AppDbContext _context;

        public RandevuController(AppDbContext context)
        {
            _context = context;
        }

        // Adım 1: İşlem Seçimi
        public async Task<IActionResult> IslemSec()
        {
            var uzmanliklar = await _context.Uzmanliklar
                .Include(u => u.Islemler)
                .ToListAsync();

            return View(uzmanliklar);
        }

        // Adım 2: Personel Seçimi
        public async Task<IActionResult> PersonelSec(int islemId)
        {
            var islem = await _context.Islemler
                .Include(i => i.Uzmanlik)
                    .ThenInclude(u => u.Personeller)
                .FirstOrDefaultAsync(i => i.Id == islemId);

            if (islem == null || islem.Uzmanlik == null)
            {
                return RedirectToAction("IslemSec");
            }

            var personeller = islem.Uzmanlik.Personeller.ToList();

            if (!personeller.Any())
            {
                ViewBag.HataMesaji = "Seçtiğiniz hizmete ait çalışan bulunmamaktadır.";
                ViewBag.Islem = islem;
                return View(new List<Personel>());
            }

            ViewBag.Islem = islem;
            return View(personeller);
        }

        // Adım 3: Tarih ve Saat Seçimi
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> TarihSaatSec(int islemId, int personelId)
        {
            var islem = await _context.Islemler.FindAsync(islemId);
            var personel = await _context.Personeller
                .Include(p => p.Mesailer)
                .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (islem == null || personel == null)
            {
                return RedirectToAction("PersonelSec", new { islemId });
            }

            ViewBag.Islem = islem;
            ViewBag.Personel = personel;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UygunlukKontrol(int islemId, int personelId, DateTime randevuTarihi, TimeSpan randevuSaati)
        {
            var islem = await _context.Islemler.FindAsync(islemId);
            var personel = await _context.Personeller
                .Include(p => p.Mesailer)
                .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (islem == null || personel == null)
            {
                TempData["ErrorMessage"] = "Geçersiz işlem veya personel seçimi.";
                return RedirectToAction("TarihSaatSec", new { islemId, personelId });
            }

            var randevuBitisZamani = randevuSaati + islem.Sure;
            var gun = randevuTarihi.DayOfWeek;
            var mesai = personel.Mesailer
                .FirstOrDefault(m => m.CalistigiGunler.Any(g => g.Gun == gun) &&
                                     m.BaslangicZamani <= randevuSaati &&
                                     m.BitisZamani >= randevuBitisZamani);

            if (mesai == null)
            {
                TempData["ErrorMessage"] = "Personelin seçilen tarihte ve saatte mesaisi bulunmamaktadır.";
                return RedirectToAction("TarihSaatSec", new { islemId, personelId });
            }

            var mevcutRandevular = await _context.Randevular
                .Where(r => r.PersonelId == personelId &&
                            r.RandevuTarihi.Date == randevuTarihi.Date)
                .ToListAsync();

            var cakisanRandevu = mevcutRandevular
                .FirstOrDefault(r =>
                    (r.RandevuSaati >= randevuSaati && r.RandevuSaati < randevuBitisZamani) ||
                    (r.RandevuSaati + r.Sure > randevuSaati));

            if (cakisanRandevu != null)
            {
                TempData["ErrorMessage"] = "Seçilen saat aralığında personelin başka bir randevusu bulunmaktadır.";
                return RedirectToAction("TarihSaatSec", new { islemId, personelId });
            }

            // Uygunluk onayı
            TempData["SuccessMessage"] = "Randevu seçimi başarılı!";
            return RedirectToAction("RandevuOnayla", new { islemId, personelId, randevuTarihi, randevuSaati });
        }


        public async Task<IActionResult> RandevuOnayla(int islemId, int personelId, DateTime randevuTarihi, TimeSpan randevuSaati)
        {
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = int.Parse(userIdString);


            var user = await _context.Users.FindAsync(userId);
            var islem = await _context.Islemler.FindAsync(islemId);
            var personel = await _context.Personeller.FindAsync(personelId);

            if (user == null || islem == null || personel == null)
            {
                return RedirectToAction("UserDashboard", "User");
            }

            var randevu = new Randevu
            {
                User = user,
                Islem = islem,
                Personel = personel,
                RandevuTarihi = randevuTarihi,
                RandevuSaati = randevuSaati,
                Durum = "Beklemede",
                Sure = islem.Sure,
                Ucret = islem.Ucret
            };

            return View(randevu);
        }

        [HttpPost]
        public async Task<IActionResult> RandevuOnayla(Randevu randevu)
        {
               
                randevu.Durum = "Beklemede"; // Onay bekleniyor
                _context.Randevular.Add(randevu);
                await _context.SaveChangesAsync();

                return RedirectToAction("Randevularim", "User");
          
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuSil(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction("Randevularim", "User"); // Kullanıcının randevu listesini gösteren bir action
        }




    }
}

