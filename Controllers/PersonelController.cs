using Microsoft.AspNetCore.Mvc;
using Fitness_Center_Web_Project.Context;
using Microsoft.EntityFrameworkCore;
using Fitness_Center_Web_Project.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Fitness_Center_Web_Project.Controllers
{
    public class PersonelController : Controller
    {
        private readonly AppDbContext _context;

        public PersonelController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult CheckAdminRole()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin")
            {
                return null; // Admin ise herhangi bir işlem yapmadan devam et
            }
            return RedirectToAction("UserDashboard", "User"); // Kullanıcı paneline yönlendir
        }


        // Personel Listeleme
        public async Task<IActionResult> Listele()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var personeller = await _context.Personeller
                .Include(p => p.Uzmanliklar)
                .ThenInclude(u => u.Islemler)
                .Include(p => p.Mesailer)
                .ThenInclude(m => m.CalistigiGunler)
                .ToListAsync();

            return View(personeller);
        }



        // GET: Personel/Create
        public async Task<IActionResult> Create()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            ViewBag.Uzmanliklar = await _context.Uzmanliklar
                .Select(u => new
                {
                    u.Id,
                    u.UzmanlikAdi
                }).ToListAsync();

            return View();
        }

        // POST: Personel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Personel personel, List<int> seciliUzmanliklar, string MesaiBaslangic, string MesaiBitis, List<DayOfWeek> CalismaGunleri)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (ModelState.IsValid)
            {
                if (seciliUzmanliklar != null && seciliUzmanliklar.Any())
                {
                    var uzmanliklar = await _context.Uzmanliklar
                        .Where(u => seciliUzmanliklar.Contains(u.Id))
                        .ToListAsync();

                    foreach (var uzmanlik in uzmanliklar)
                    {
                        personel.Uzmanliklar.Add(uzmanlik);
                    }
                }

                // Mesai saatlerini doğru şekilde dönüştür
                if (TimeSpan.TryParse(MesaiBaslangic, out TimeSpan mesaiBaslangic) && TimeSpan.TryParse(MesaiBitis, out TimeSpan mesaiBitis))
                {
                    var mesai = new Mesai
                    {
                        BaslangicZamani = mesaiBaslangic,
                        BitisZamani = mesaiBitis
                    };

                    if (CalismaGunleri != null && CalismaGunleri.Any())
                    {
                        mesai.CalistigiGunler = CalismaGunleri.Select(gun => new MesaiGunu
                        {
                            Gun = gun
                        }).ToList();
                    }

                    personel.Mesailer.Add(mesai);
                }

                // Personeli kaydet
                _context.Personeller.Add(personel);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Personel başarıyla eklendi.";
                return RedirectToAction(nameof(Listele));
            }

            // Hata durumunda uzmanlıkları tekrar doldur
            ViewBag.Uzmanliklar = await _context.Uzmanliklar
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UzmanlikAdi
                }).ToListAsync();

            return View(personel);
        }





        // GET: Personel/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var personel = await _context.Personeller
                .Include(p => p.Uzmanliklar)
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null)
            {
                return NotFound();
            }

            // Uzmanlıkları doldur
            var personelUzmanlikIds = personel.Uzmanliklar.Select(uzmanlik => uzmanlik.Id).ToList();
            ViewBag.Uzmanliklar = await _context.Uzmanliklar
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UzmanlikAdi,
                    Selected = personelUzmanlikIds.Contains(u.Id)
                }).ToListAsync();

            return View(personel);
        }

        // POST: Personel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Personel personel, List<int> seciliUzmanliklar, string MesaiBaslangic, string MesaiBitis, List<DayOfWeek> CalismaGunleri)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (id != personel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var mevcutPersonel = await _context.Personeller
                    .Include(p => p.Uzmanliklar)
                    .Include(p => p.Mesailer)
                        .ThenInclude(m => m.CalistigiGunler)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (mevcutPersonel == null)
                {
                    return NotFound();
                }

                // Uzmanlıkları güncelle
                mevcutPersonel.Uzmanliklar.Clear();
                if (seciliUzmanliklar != null && seciliUzmanliklar.Any())
                {
                    var uzmanliklar = await _context.Uzmanliklar
                        .Where(u => seciliUzmanliklar.Contains(u.Id))
                        .ToListAsync();

                    foreach (var uzmanlik in uzmanliklar)
                    {
                        mevcutPersonel.Uzmanliklar.Add(uzmanlik);
                    }
                }

                // Mesai saatlerini güncelle
                if (TimeSpan.TryParse(MesaiBaslangic, out TimeSpan mesaiBaslangic) && TimeSpan.TryParse(MesaiBitis, out TimeSpan mesaiBitis))
                {
                    var mevcutMesai = mevcutPersonel.Mesailer.FirstOrDefault();
                    if (mevcutMesai != null)
                    {
                        mevcutMesai.BaslangicZamani = mesaiBaslangic;
                        mevcutMesai.BitisZamani = mesaiBitis;

                        // Çalışma günlerini güncelle
                        mevcutMesai.CalistigiGunler.Clear();
                        if (CalismaGunleri != null && CalismaGunleri.Any())
                        {
                            mevcutMesai.CalistigiGunler = CalismaGunleri.Select(gun => new MesaiGunu { Gun = gun }).ToList();
                        }
                    }
                    else
                    {
                        mevcutPersonel.Mesailer.Add(new Mesai
                        {
                            BaslangicZamani = mesaiBaslangic,
                            BitisZamani = mesaiBitis,
                            CalistigiGunler = CalismaGunleri?.Select(gun => new MesaiGunu { Gun = gun }).ToList()
                        });
                    }
                }

                mevcutPersonel.Ad = personel.Ad;
                mevcutPersonel.Soyad = personel.Soyad;
                mevcutPersonel.Cinsiyet = personel.Cinsiyet;

                _context.Update(mevcutPersonel);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Personel başarıyla güncellendi.";
                return RedirectToAction(nameof(Listele));
            }

            // Hata durumunda uzmanlıkları tekrar doldur
            ViewBag.Uzmanliklar = await _context.Uzmanliklar
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UzmanlikAdi
                }).ToListAsync();

            return View(personel);
        }



        // GET: Personel/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var personel = await _context.Personeller
                .Include(p => p.Uzmanliklar)
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null)
            {
                return NotFound();
            }

            return View(personel);
        }

        // POST: Personel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var personel = await _context.Personeller
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null)
            {
                return NotFound();
            }

            // Mesai ve çalışma günlerini kaldır
            if (personel.Mesailer != null)
            {
                foreach (var mesai in personel.Mesailer)
                {
                    _context.MesaiGunleri.RemoveRange(mesai.CalistigiGunler);
                }
                _context.Mesailer.RemoveRange(personel.Mesailer);
            }

            // Personeli sil
            _context.Personeller.Remove(personel);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Personel başarıyla silindi.";
            return RedirectToAction(nameof(Listele));
        }


        public async Task<IActionResult> Randevular(int id)
        {
            // İlgili personelin onaylanmış randevularını alıyoruz
            var randevular = await _context.Randevular
                .Include(r => r.User) // Kullanıcı bilgisi gerekiyorsa eklenebilir
                .Include(r => r.Islem) // İşlem bilgisi gerekiyorsa eklenebilir
                .Where(r => r.PersonelId == id && r.Durum == "Onaylandı") // Sadece onaylanmış randevular
                .ToListAsync();

            // Personelin bilgilerini de alıyoruz (isteğe bağlı)
            var personel = await _context.Personeller.FirstOrDefaultAsync(p => p.Id == id);
            if (personel == null)
            {
                return NotFound();
            }

            ViewBag.Personel = personel; // Personel bilgilerini View'e gönderiyoruz
            return View(randevular); // Randevuları View'e gönderiyoruz
        }




    }
}
