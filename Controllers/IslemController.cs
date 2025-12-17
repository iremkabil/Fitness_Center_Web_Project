using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Center_Web_Project.Controllers
{
    // Islem = Fitness tarafında "Hizmet/Seans" CRUD
    public class IslemController : Controller
    {
        private readonly AppDbContext _context;

        public IslemController(AppDbContext context)
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

            var islemler = await _context.Islemler
                .AsNoTracking()
                .OrderByDescending(x => x.AktifMi)
                .ThenBy(x => x.Ad)
                .ToListAsync();

            return View(islemler);
        }

        // Yeni hizmet/seans oluşturma (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            // Eğer Create view'ında uzmanlık seçimi eklemek istersen:
            // ViewBag.Uzmanliklar = await _context.Uzmanliklar.AsNoTracking().ToListAsync();
            return View();
        }

        // Yeni hizmet/seans oluşturma (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Islem islem)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (!ModelState.IsValid)
                return View(islem);

            // Eski projede otomatik null yapıyordun; fitness'ta da zorunlu değil.
            // View'da seçim yoksa null kalsın.
            if (islem.UzmanlikId == 0) islem.UzmanlikId = null;

            _context.Islemler.Add(islem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Listele));
        }

        // Düzenleme (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (id is null) return NotFound();

            var islem = await _context.Islemler.FindAsync(id.Value);
            if (islem is null) return NotFound();

            return View(islem);
        }

        // Düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Islem islem)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (id != islem.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(islem);

            var mevcutIslem = await _context.Islemler.FindAsync(id);
            if (mevcutIslem is null) return NotFound();

            // Uzmanlık alanını view'dan yönetmiyorsan koru
            var mevcutUzmanlikId = mevcutIslem.UzmanlikId;

            // Güncellenecek alanlar
            mevcutIslem.Ad = islem.Ad;
            mevcutIslem.Ucret = islem.Ucret;
            mevcutIslem.Sure = islem.Sure;
            mevcutIslem.Aciklama = islem.Aciklama;
            mevcutIslem.AktifMi = islem.AktifMi;

            // Korunan alan
            mevcutIslem.UzmanlikId = mevcutUzmanlikId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Listele));
        }

        // Silme (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (id is null) return NotFound();

            var islem = await _context.Islemler
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (islem is null) return NotFound();

            return View(islem);
        }

        // Silme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var islem = await _context.Islemler.FindAsync(id);
            if (islem != null)
            {
                _context.Islemler.Remove(islem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Listele));
        }
    }
}
