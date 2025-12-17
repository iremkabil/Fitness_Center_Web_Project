using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Center_Web_Project.Controllers
{
    public class RandevuController : Controller
    {
        private readonly AppDbContext _context;

        public RandevuController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? CheckUserRole()
        {
            var role = HttpContext.Session.GetString("Role");
            // Admin burada randevu almayacak; kullanıcı girişi şart
            if (string.IsNullOrWhiteSpace(role) || role == "Admin")
                return RedirectToAction("Login", "Account");

            return null;
        }

        // Adım 1: Hizmet/Seans seçimi
        [HttpGet]
        public async Task<IActionResult> IslemSec()
        {
            var roleCheck = CheckUserRole();
            if (roleCheck != null) return roleCheck;

            var uzmanliklar = await _context.Uzmanliklar
                .AsNoTracking()
                .Where(u => u.AktifMi)
                .Include(u => u.Islemler.Where(i => i.AktifMi))
                .OrderBy(u => u.UzmanlikAdi)
                .ToListAsync();

            return View(uzmanliklar);
        }

        // Adım 2: Antrenör seçimi
        [HttpGet]
        public async Task<IActionResult> PersonelSec(int islemId)
        {
            var roleCheck = CheckUserRole();
            if (roleCheck != null) return roleCheck;

            var islem = await _context.Islemler
                .AsNoTracking()
                .Include(i => i.Uzmanlik)
                    .ThenInclude(u => u.Personeller)
                .FirstOrDefaultAsync(i => i.Id == islemId);

            if (islem == null || islem.Uzmanlik == null)
                return RedirectToAction(nameof(IslemSec));

            var personeller = islem.Uzmanlik.Personeller.ToList();

            if (!personeller.Any())
            {
                ViewBag.HataMesaji = "Seçtiğiniz hizmete ait antrenör bulunmamaktadır.";
                ViewBag.Islem = islem;
                return View(new List<Personel>());
            }

            ViewBag.Islem = islem;
            return View(personeller);
        }

        // Adım 3: Tarih ve saat seçimi (GET)
        [HttpGet]
        public async Task<IActionResult> TarihSaatSec(int islemId, int personelId)
        {
            var roleCheck = CheckUserRole();
            if (roleCheck != null) return roleCheck;

            var islem = await _context.Islemler.AsNoTracking().FirstOrDefaultAsync(x => x.Id == islemId);
            var personel = await _context.Personeller
                .AsNoTracking()
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (islem == null || personel == null)
                return RedirectToAction(nameof(PersonelSec), new { islemId });

            ViewBag.Islem = islem;
            ViewBag.Personel = personel;

            return View();
        }

        // Uygunluk kontrolü (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UygunlukKontrol(int islemId, int personelId, DateTime randevuTarihi, TimeSpan randevuSaati)
        {
            var roleCheck = CheckUserRole();
            if (roleCheck != null) return roleCheck;

            var islem = await _context.Islemler.FirstOrDefaultAsync(i => i.Id == islemId);
            var personel = await _context.Personeller
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (islem == null || personel == null)
            {
                TempData["ErrorMessage"] = "Geçersiz hizmet veya antrenör seçimi.";
                return RedirectToAction(nameof(TarihSaatSec), new { islemId, personelId });
            }

            // 1) Geçmiş tarih/saat engeli (kritik)
            var baslangic = randevuTarihi.Date.Add(randevuSaati);
            if (baslangic < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Geçmiş bir tarih/saat için randevu oluşturamazsınız.";
                return RedirectToAction(nameof(TarihSaatSec), new { islemId, personelId });
            }

            // 2) Mesai kontrolü
            var bitisSaat = randevuSaati + islem.Sure;
            var gun = randevuTarihi.DayOfWeek;

            var mesaiUygunMu = personel.Mesailer.Any(m =>
                m.CalistigiGunler.Any(g => g.Gun == gun) &&
                m.BaslangicZamani <= randevuSaati &&
                m.BitisZamani >= bitisSaat);

            if (!mesaiUygunMu)
            {
                TempData["ErrorMessage"] = "Antrenörün seçilen tarihte/saatte mesaisi bulunmamaktadır.";
                return RedirectToAction(nameof(TarihSaatSec), new { islemId, personelId });
            }

            // 3) Çakışma kontrolü (overlap)
            var gunRandevulari = await _context.Randevular
                .Where(r => r.PersonelId == personelId
                            && r.RandevuTarihi.Date == randevuTarihi.Date
                            && r.Durum != "İptal")
                .ToListAsync();

            var yeniBaslangic = randevuSaati;
            var yeniBitis = randevuSaati + islem.Sure;

            bool cakismaVar = gunRandevulari.Any(r =>
            {
                var mevcutBaslangic = r.RandevuSaati;
                var mevcutBitis = r.RandevuSaati + r.Sure;
                return yeniBaslangic < mevcutBitis && mevcutBaslangic < yeniBitis;
            });

            if (cakismaVar)
            {
                TempData["ErrorMessage"] = "Seçilen saat aralığında antrenörün başka bir randevusu bulunmaktadır.";
                return RedirectToAction(nameof(TarihSaatSec), new { islemId, personelId });
            }

            TempData["SuccessMessage"] = "Seçim uygun. Randevu onay sayfasına yönlendiriliyorsunuz.";
            return RedirectToAction(nameof(RandevuOnayla), new { islemId, personelId, randevuTarihi, randevuSaati });
        }

        // Onay ekranı (GET)
        [HttpGet]
        public async Task<IActionResult> RandevuOnayla(int islemId, int personelId, DateTime randevuTarihi, TimeSpan randevuSaati)
        {
            var roleCheck = CheckUserRole();
            if (roleCheck != null) return roleCheck;

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            var islem = await _context.Islemler.AsNoTracking().FirstOrDefaultAsync(i => i.Id == islemId);
            var personel = await _context.Personeller.AsNoTracking().FirstOrDefaultAsync(p => p.Id == personelId);

            if (user == null || islem == null || personel == null)
                return RedirectToAction("UserDashboard", "User");

            var randevu = new Randevu
            {
                UserId = userId,
                User = user,
                IslemId = islemId,
                Islem = islem,
                PersonelId = personelId,
                Personel = personel,
                RandevuTarihi = randevuTarihi.Date,
                RandevuSaati = randevuSaati,
                Durum = "Beklemede",
                Sure = islem.Sure,
                Ucret = islem.Ucret
            };

            return View(randevu);
        }

        // Onay (POST) -> DB’ye yazma burada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOnayla(int islemId, int personelId, DateTime randevuTarihi, TimeSpan randevuSaati)
        {
            var roleCheck = CheckUserRole();
            if (roleCheck != null) return roleCheck;

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Account");

            var islem = await _context.Islemler.FirstOrDefaultAsync(i => i.Id == islemId);
            var personel = await _context.Personeller
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (islem == null || personel == null)
                return RedirectToAction(nameof(IslemSec));

            // Güvenlik: POST’ta tekrar geçmiş/mesai/çakışma kontrolü
            var baslangic = randevuTarihi.Date.Add(randevuSaati);
            if (baslangic < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Geçmiş bir tarih/saat için randevu oluşturamazsınız.";
                return RedirectToAction(nameof(TarihSaatSec), new { islemId, personelId });
            }

            var bitisSaat = randevuSaati + islem.Sure;
            var gun = randevuTarihi.DayOfWeek;

            var mesaiUygunMu = personel.Mesailer.Any(m =>
                m.CalistigiGunler.Any(g => g.Gun == gun) &&
                m.BaslangicZamani <= randevuSaati &&
                m.BitisZamani >= bitisSaat);

            if (!mesaiUygunMu)
            {
                TempData["ErrorMessage"] = "Antrenörün seçilen tarihte/saatte mesaisi bulunmamaktadır.";
                return RedirectToAction(nameof(TarihSaatSec), new { islemId, personelId });
            }

            var gunRandevulari = await _context.Randevular
                .Where(r => r.PersonelId == personelId
                            && r.RandevuTarihi.Date == randevuTarihi.Date
                            && r.Durum != "İptal")
                .ToListAsync();

            var yeniBaslangic = randevuSaati;
            var yeniBitis = randevuSaati + islem.Sure;

            bool cakismaVar = gunRandevulari.Any(r =>
            {
                var mevcutBaslangic = r.RandevuSaati;
                var mevcutBitis = r.RandevuSaati + r.Sure;
                return yeniBaslangic < mevcutBitis && mevcutBaslangic < yeniBitis;
            });

            if (cakismaVar)
            {
                TempData["ErrorMessage"] = "Seçilen saat aralığında antrenörün başka bir randevusu bulunmaktadır.";
                return RedirectToAction(nameof(TarihSaatSec), new { islemId, personelId });
            }

            var yeniRandevu = new Randevu
            {
                UserId = userId,
                PersonelId = personelId,
                IslemId = islemId,
                RandevuTarihi = randevuTarihi.Date,
                RandevuSaati = randevuSaati,
                Durum = "Beklemede",
                Sure = islem.Sure,
                Ucret = islem.Ucret
            };

            _context.Randevular.Add(yeniRandevu);
            await _context.SaveChangesAsync();

            return RedirectToAction("Randevularim", "User");
        }

        // Kullanıcı randevu silme (kendi randevusu ise)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuSil(int id)
        {
            var roleCheck = CheckUserRole();
            if (roleCheck != null) return roleCheck;

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Account");

            var randevu = await _context.Randevular.FirstOrDefaultAsync(r => r.Id == id);
            if (randevu == null) return NotFound();

            // Güvenlik: sadece kendi randevusunu silebilsin
            if (randevu.UserId != userId)
                return Forbid();

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction("Randevularim", "User");
        }
    }
}
