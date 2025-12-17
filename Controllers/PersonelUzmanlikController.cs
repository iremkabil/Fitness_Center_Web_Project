using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Center_Web_Project.Controllers
{
    // PersonelUzmanlik = Fitness'ta "Branş/Uzmanlık" (Yoga, Pilates, PT vb.)
    public class PersonelUzmanlikController : Controller
    {
        private readonly AppDbContext _context;

        public PersonelUzmanlikController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? CheckAdminRole()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return null;

            return RedirectToAction("Login", "Account");
        }

        // Listeleme
        [HttpGet]
        public async Task<IActionResult> Listele()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var uzmanliklar = await _context.Uzmanliklar
                .AsNoTracking()
                .Include(u => u.Islemler)
                .OrderByDescending(u => u.AktifMi)
                .ThenBy(u => u.UzmanlikAdi)
                .ToListAsync();

            return View(uzmanliklar);
        }

        // Yeni uzmanlık oluşturma (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            await FillIslemlerForView();
            return View();
        }

        // Yeni uzmanlık oluşturma (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonelUzmanlik model, List<int> seciliIslemler)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (string.IsNullOrWhiteSpace(model.UzmanlikAdi))
                ModelState.AddModelError(nameof(PersonelUzmanlik.UzmanlikAdi), "Uzmanlık adı zorunludur.");

            // Aynı isim kontrolü (case-insensitive)
            var name = (model.UzmanlikAdi ?? "").Trim();
            bool exists = await _context.Uzmanliklar.AnyAsync(u => u.UzmanlikAdi.ToLower() == name.ToLower());
            if (exists)
                ModelState.AddModelError(nameof(PersonelUzmanlik.UzmanlikAdi), "Bu isimde bir uzmanlık zaten mevcut.");

            if (seciliIslemler == null || !seciliIslemler.Any())
                ModelState.AddModelError("Islemler", "En az bir hizmet/seans seçmelisiniz.");

            if (!ModelState.IsValid)
            {
                await FillIslemlerForView(selectedIds: seciliIslemler);
                return View(model);
            }

            model.UzmanlikAdi = name;

            // Seçili işlemleri ilişkilendir
            model.Islemler = await _context.Islemler
                .Where(i => seciliIslemler.Contains(i.Id))
                .ToListAsync();

            _context.Uzmanliklar.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Listele));
        }

        // Edit (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var uzmanlik = await _context.Uzmanliklar
                .Include(u => u.Islemler)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlik == null) return NotFound();

            var selectedIds = uzmanlik.Islemler.Select(i => i.Id).ToList();
            await FillIslemlerForView(selectedIds);

            return View(uzmanlik);
        }

        // Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonelUzmanlik model, List<int> seciliIslemler)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (id != model.Id) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.UzmanlikAdi))
                ModelState.AddModelError(nameof(PersonelUzmanlik.UzmanlikAdi), "Uzmanlık adı zorunludur.");

            if (seciliIslemler == null || !seciliIslemler.Any())
                ModelState.AddModelError("Islemler", "En az bir hizmet/seans seçmelisiniz.");

            var name = (model.UzmanlikAdi ?? "").Trim();

            // Edit sırasında aynı isim kontrolü (kendi kaydı hariç)
            bool exists = await _context.Uzmanliklar.AnyAsync(u =>
                u.Id != id && u.UzmanlikAdi.ToLower() == name.ToLower());

            if (exists)
                ModelState.AddModelError(nameof(PersonelUzmanlik.UzmanlikAdi), "Bu isimde bir uzmanlık zaten mevcut.");

            if (!ModelState.IsValid)
            {
                await FillIslemlerForView(selectedIds: seciliIslemler);
                return View(model);
            }

            var uzmanlik = await _context.Uzmanliklar
                .Include(u => u.Islemler)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlik == null) return NotFound();

            uzmanlik.UzmanlikAdi = name;
            uzmanlik.AktifMi = model.AktifMi;

            uzmanlik.Islemler = await _context.Islemler
                .Where(i => seciliIslemler.Contains(i.Id))
                .ToListAsync();

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Listele));
        }

        // Delete (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var uzmanlik = await _context.Uzmanliklar
                .AsNoTracking()
                .Include(u => u.Islemler)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlik == null) return NotFound();

            return View(uzmanlik);
        }

        // Delete (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var uzmanlik = await _context.Uzmanliklar
                .Include(u => u.Islemler)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlik == null) return NotFound();

            // Bu uzmanlığa bağlı işlemlerin FK'sini null'la (FK varsa)
            var islemler = await _context.Islemler
                .Where(i => i.UzmanlikId == id)
                .ToListAsync();

            foreach (var islem in islemler)
                islem.UzmanlikId = null;

            _context.Uzmanliklar.Remove(uzmanlik);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Listele));
        }

        private async Task FillIslemlerForView(List<int>? selectedIds = null)
        {
            var selected = (selectedIds ?? new List<int>()).ToHashSet();

            ViewBag.Islemler = await _context.Islemler
                .AsNoTracking()
                .Where(i => i.AktifMi) // sadece aktif hizmetler
                .OrderBy(i => i.Ad)
                .Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = $"{i.Ad} ({i.Sure.TotalMinutes:0} dk)",
                    Selected = selected.Contains(i.Id)
                })
                .ToListAsync();
        }
    }
}
//PersonelUzmanlik modeline AktifMi alanını eklemiştik; bu controller onu kullanıyor.