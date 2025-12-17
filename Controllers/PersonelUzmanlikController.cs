using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fitness_Center_Web_Project.Models;
using Fitness_Center_Web_Project.Context;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Fitness_Center_Web_Project.Controllers
{
    public class PersonelUzmanlikController : Controller
    {
        private readonly AppDbContext _context;

        public PersonelUzmanlikController(AppDbContext context)
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

        // Listeleme
        public async Task<IActionResult> Listele()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var uzmanliklar = await _context.Uzmanliklar
                .Include(u => u.Islemler)
                .ToListAsync();

            return View(uzmanliklar);
        }

        // Yeni uzmanlık oluşturma (GET)
        public async Task<IActionResult> Create()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            ViewBag.Islemler = await _context.Islemler.ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonelUzmanlik model, List<int> seciliIslemler)
        {
            if (string.IsNullOrWhiteSpace(model.UzmanlikAdi))
            {
                ModelState.AddModelError("UzmanlikAdi", "Uzmanlık adı zorunludur.");
            }

            // Aynı isimde uzmanlık kontrolü
            var mevcutUzmanlik = await _context.Uzmanliklar
                .FirstOrDefaultAsync(u => u.UzmanlikAdi.ToLower() == model.UzmanlikAdi.ToLower());
            if (mevcutUzmanlik != null)
            {
                ModelState.AddModelError("UzmanlikAdi", "Bu isimde bir uzmanlık zaten mevcut.");
            }

            if (!ModelState.IsValid)
            {
                // Hata durumunda işlemleri tekrar doldur
                ViewBag.Islemler = await _context.Islemler.ToListAsync();
                return View(model);
            }

            // Seçili işlemleri ilişkilendir
            model.Islemler = await _context.Islemler
                .Where(i => seciliIslemler.Contains(i.Id))
                .ToListAsync();

            _context.Uzmanliklar.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Listele));
        }

        // Edit Uzmanlık (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var uzmanlik = await _context.Uzmanliklar
                .Include(u => u.Islemler)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlik == null)
            {
                return NotFound();
            }

            ViewBag.Islemler = await _context.Islemler.ToListAsync() ?? new List<Islem>();
            return View(uzmanlik);
        }


        // Edit Uzmanlık (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonelUzmanlik model, List<int> seciliIslemler)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(model.UzmanlikAdi))
            {
                ModelState.AddModelError("UzmanlikAdi", "Uzmanlık adı zorunludur.");
            }

            if (seciliIslemler == null || !seciliIslemler.Any())
            {
                ModelState.AddModelError("Islemler", "En az bir işlem seçmelisiniz.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Islemler = await _context.Islemler.ToListAsync();
                return View(model);
            }

            var uzmanlik = await _context.Uzmanliklar
                .Include(u => u.Islemler)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlik == null)
            {
                return NotFound();
            }

            uzmanlik.UzmanlikAdi = model.UzmanlikAdi;
            uzmanlik.Islemler = await _context.Islemler
                .Where(i => seciliIslemler.Contains(i.Id))
                .ToListAsync();

            _context.Uzmanliklar.Update(uzmanlik);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Listele));
        }


        // Silme Onayı Sayfası (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck; 

            var uzmanlik = await _context.Uzmanliklar
                .Include(u => u.Islemler)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlik == null)
            {
                return NotFound();
            }

            return View(uzmanlik);
        }

        // Silme İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uzmanlik = await _context.Uzmanliklar
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlik == null)
            {
                return NotFound();
            }

            // İlgili işlemlerin UzmanlikId değerini null yap
            var islemler = await _context.Islemler
                .Where(i => i.UzmanlikId == id)
                .ToListAsync();

            foreach (var islem in islemler)
            {
                islem.UzmanlikId = null;
            }

            _context.Uzmanliklar.Remove(uzmanlik);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Listele));
        }




    }
}
