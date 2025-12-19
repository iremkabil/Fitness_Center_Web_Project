using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        // Adım 2: Personel Seçimi
        [HttpGet]
        public async Task<IActionResult> PersonelSec(int islemId)
        {
            var islem = await _context.Islemler
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == islemId);

            if (islem == null)
                return RedirectToAction("IslemSec");

            // Kritik düzeltme:
            // Personeli, Personeller üzerinden; personelin uzmanlıklarında bu işlem var mı diye çekiyoruz.
            var personeller = await _context.Personeller
                .Include(p => p.Uzmanliklar)
                    .ThenInclude(u => u.Islemler)
                .Where(p => p.AktifMi &&
                            p.Uzmanliklar.Any(u => u.AktifMi &&
                                                   u.Islemler.Any(i => i.Id == islemId)))
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Islem = islem;

            if (!personeller.Any())
            {
                ViewBag.HataMesaji = "Seçtiğiniz hizmete ait çalışan bulunmamaktadır.";
                return View(new List<Personel>());
            }

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

        // GET: /Randevu/UygunlukKontrol?islemId=1&personelId=2&tarih=2025-12-20&saat=12:00
        [HttpGet]
        public async Task<IActionResult> UygunlukKontrol(int islemId, int personelId, DateTime tarih, TimeSpan saat)
        {
            if (islemId <= 0 || personelId <= 0)
                return BadRequest("islemId/personelId eksik.");

            // tarih/saat birleşimi
            var baslangic = tarih.Date.Add(saat);

            // İşlem ve personel var mı?
            var islem = await _context.Islemler.AsNoTracking().FirstOrDefaultAsync(i => i.Id == islemId);
            var personel = await _context.Personeller
                .AsNoTracking()
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (islem == null || personel == null)
                return BadRequest("İşlem veya personel bulunamadı.");

            // 1) Personelin çalışma günü kontrolü
            var gun = baslangic.DayOfWeek;
            var mesai = personel.Mesailer?.FirstOrDefault(); // sende tek mesai var gibi
            if (mesai == null) return BadRequest("Personelin mesai bilgisi yok.");

            bool gunUygun = mesai.CalistigiGunler != null && mesai.CalistigiGunler.Any(g => g.Gun == gun);
            if (!gunUygun)
            {
                TempData["ErrorMessage"] = "Seçilen gün personelin çalışma günü değil.";
                return RedirectToAction("TarihSaatSec", new { islemId, personelId });
            }

            // 2) Mesai saat aralığı kontrolü
            var bitis = baslangic.Add(islem.Sure);
            var mesaiBas = baslangic.Date.Add(mesai.BaslangicZamani);
            var mesaiBit = baslangic.Date.Add(mesai.BitisZamani);

            if (baslangic < mesaiBas || bitis > mesaiBit)
            {
                TempData["ErrorMessage"] = "Seçilen saat personelin mesai saatleri dışında.";
                return RedirectToAction("TarihSaatSec", new { islemId, personelId });
            }

            // Aynı personelin aynı gün randevularını çek (Beklemede/Onaylandı)
            var gunRandevulari = await _context.Randevular
                .AsNoTracking()
                .Where(r =>
                    r.PersonelId == personelId &&
                    r.RandevuTarihi.Date == tarih.Date &&
                    (r.Durum == "Beklemede" || r.Durum == "Onaylandı"))
                .ToListAsync();

            // Çakışma kontrolü (C# tarafında)
            bool cakismaVar = gunRandevulari.Any(r =>
            {
                var rBas = r.RandevuTarihi.Date.Add(r.RandevuSaati);
                var rBit = rBas.Add(r.Sure);
                return rBas < bitis && rBit > baslangic;
            });

            if (cakismaVar)
            {
                TempData["ErrorMessage"] = "Bu saat dolu. Lütfen başka bir saat seçin.";
                return RedirectToAction("TarihSaatSec", new { islemId, personelId });
            }


            // uygunsa onay sayfasına götür (senin projende hangi action ise onu çağır)
            return RedirectToAction("RandevuOnayla", new { islemId, personelId, randevuTarihi = baslangic.ToString("yyyy-MM-dd"), randevuSaati = baslangic.ToString("HH:mm") });
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
        public async Task<IActionResult> RandevuOnaylaPost(int islemId, int personelId, DateTime randevuTarihi, TimeSpan randevuSaati)
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