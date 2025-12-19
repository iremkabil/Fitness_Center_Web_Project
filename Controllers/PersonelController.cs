using Microsoft.AspNetCore.Mvc;
using Fitness_Center_Web_Project.Context;
using Microsoft.EntityFrameworkCore;
using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fitness_Center_Web_Project.Controllers
{
    public class PersonelController : Controller
    {
        private readonly AppDbContext _context;

        public PersonelController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? CheckAdminRole()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return null;

            return RedirectToAction("UserDashboard", "User");
        }

        private async Task FillUzmanliklarForView(List<int>? selectedIds = null)
        {
            var selected = (selectedIds ?? new List<int>()).ToHashSet();

            ViewBag.Uzmanliklar = await _context.Uzmanliklar
                .AsNoTracking()
                .Where(u => u.AktifMi)
                .OrderBy(u => u.UzmanlikAdi)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UzmanlikAdi,
                    Selected = selected.Contains(u.Id)
                })
                .ToListAsync();
        }

        // Personel Listeleme
        [HttpGet]
        public async Task<IActionResult> Listele()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var personeller = await _context.Personeller
                .Include(p => p.Uzmanliklar)
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .AsNoTracking()
                .ToListAsync();

            return View(personeller);
        }

        // GET: Personel/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            await FillUzmanliklarForView();
            return View(new Personel());
        }

        // POST: Personel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Personel personel,
            List<int> seciliUzmanliklar,
            string MesaiBaslangic,
            string MesaiBitis,
            List<DayOfWeek> CalismaGunleri)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            // En az 1 uzmanlık şartı (istersen kaldırırsın)
            if (seciliUzmanliklar == null || !seciliUzmanliklar.Any())
                ModelState.AddModelError("Uzmanliklar", "En az bir uzmanlık seçmelisiniz.");

            if (!ModelState.IsValid)
            {
                await FillUzmanliklarForView(seciliUzmanliklar);
                return View(personel);
            }

            // Uzmanlık ilişkilendir
            var uzmanliklar = await _context.Uzmanliklar
                .Where(u => seciliUzmanliklar.Contains(u.Id))
                .ToListAsync();

            personel.Uzmanliklar = uzmanliklar;

            // Mesai
            if (TimeSpan.TryParse(MesaiBaslangic, out var bas) &&
                TimeSpan.TryParse(MesaiBitis, out var bit))
            {
                var mesai = new Mesai
                {
                    BaslangicZamani = bas,
                    BitisZamani = bit,
                    CalistigiGunler = (CalismaGunleri ?? new List<DayOfWeek>())
                        .Select(g => new MesaiGunu { Gun = g })
                        .ToList()
                };

                personel.Mesailer = new List<Mesai> { mesai };
            }

            _context.Personeller.Add(personel);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Personel başarıyla eklendi.";
            return RedirectToAction(nameof(Listele));
        }

        // GET: Personel/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var personel = await _context.Personeller
                .Include(p => p.Uzmanliklar)
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null) return NotFound();

            var selectedIds = personel.Uzmanliklar?.Select(u => u.Id).ToList() ?? new List<int>();
            await FillUzmanliklarForView(selectedIds);

            return View(personel);
        }

        // POST: Personel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Personel personel,
            List<int> seciliUzmanliklar,
            string MesaiBaslangic,
            string MesaiBitis,
            List<DayOfWeek> CalismaGunleri)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (id != personel.Id) return BadRequest();

            if (seciliUzmanliklar == null || !seciliUzmanliklar.Any())
                ModelState.AddModelError("Uzmanliklar", "En az bir uzmanlık seçmelisiniz.");

            if (!ModelState.IsValid)
            {
                await FillUzmanliklarForView(seciliUzmanliklar);
                return View(personel);
            }

            var mevcut = await _context.Personeller
                .Include(p => p.Uzmanliklar)
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (mevcut == null) return NotFound();

            // Alanlar
            mevcut.Ad = personel.Ad;
            mevcut.Soyad = personel.Soyad;
            mevcut.Cinsiyet = personel.Cinsiyet;

            // Uzmanlıklar (many-to-many)
            mevcut.Uzmanliklar.Clear();
            var uzmanliklar = await _context.Uzmanliklar
                .Where(u => seciliUzmanliklar.Contains(u.Id))
                .ToListAsync();

            foreach (var u in uzmanliklar)
                mevcut.Uzmanliklar.Add(u);

            // Mesai
            if (TimeSpan.TryParse(MesaiBaslangic, out var bas) &&
                TimeSpan.TryParse(MesaiBitis, out var bit))
            {
                var mesai = mevcut.Mesailer.FirstOrDefault();
                if (mesai == null)
                {
                    mesai = new Mesai();
                    mevcut.Mesailer.Add(mesai);
                }

                mesai.BaslangicZamani = bas;
                mesai.BitisZamani = bit;

                mesai.CalistigiGunler.Clear();
                foreach (var g in (CalismaGunleri ?? new List<DayOfWeek>()))
                    mesai.CalistigiGunler.Add(new MesaiGunu { Gun = g });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Personel başarıyla güncellendi.";
            return RedirectToAction(nameof(Listele));
        }

        // GET: Personel/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var personel = await _context.Personeller
                .Include(p => p.Uzmanliklar)
                .Include(p => p.Mesailer)
                    .ThenInclude(m => m.CalistigiGunler)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null) return NotFound();
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

            if (personel == null) return NotFound();

            foreach (var mesai in personel.Mesailer)
                _context.MesaiGunleri.RemoveRange(mesai.CalistigiGunler);

            _context.Mesailer.RemoveRange(personel.Mesailer);
            _context.Personeller.Remove(personel);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Personel başarıyla silindi.";
            return RedirectToAction(nameof(Listele));
        }

        public async Task<IActionResult> Randevular(int id)
        {
            var randevular = await _context.Randevular
                .Include(r => r.User)
                .Include(r => r.Islem)
                .Where(r => r.PersonelId == id && r.Durum == "Onaylandı")
                .ToListAsync();

            var personel = await _context.Personeller.FirstOrDefaultAsync(p => p.Id == id);
            if (personel == null) return NotFound();

            ViewBag.Personel = personel;
            return View(randevular);
        }
    }
}
