using Microsoft.AspNetCore.Mvc;
using WebProje.Models;
using WebProje.Context;  
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebProje.Controllers
{
    public class IslemController : Controller
    {
        private readonly AppDbContext _context;

        public IslemController(AppDbContext context)
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

            var islemler = await _context.Islemler.ToListAsync();
            return View(islemler);
        }

        // Yeni işlem oluşturma (GET)
        public IActionResult Create()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            return View();
        }

        // Yeni işlem oluşturma (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Islem islem)
        {
            if (ModelState.IsValid)
            {
                islem.UzmanlikId = null;  // Uzmanlık ID'sini otomatik olarak null yap
                _context.Add(islem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Listele));
            }
            return View(islem);
        }

        // Düzenleme (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (id == null) return NotFound();

            var islem = await _context.Islemler.FindAsync(id);
            if (islem == null) return NotFound();

            return View(islem);
        }

        // Düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Islem islem)
        {
            if (id != islem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanındaki mevcut işlem kaydını alıyoruz
                    var mevcutIslem = await _context.Islemler.FindAsync(id);

                    if (mevcutIslem == null)
                    {
                        return NotFound();
                    }

                    // Mevcut UzmanlikId'yi koruyoruz
                    islem.UzmanlikId = mevcutIslem.UzmanlikId;

                    // Yalnızca diğer alanları güncelliyoruz
                    _context.Entry(mevcutIslem).CurrentValues.SetValues(islem);

                    // Değişiklikleri kaydediyoruz
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IslemExists(islem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Listele));
            }
            return View(islem);
        }

        // Silme (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            if (id == null) return NotFound();

            var islem = await _context.Islemler.FirstOrDefaultAsync(m => m.Id == id);
            if (islem == null) return NotFound();

            return View(islem);
        }

        // Silme (POST)
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var islem = await _context.Islemler.FindAsync(id);
            if (islem != null)
            {
                _context.Islemler.Remove(islem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Listele));
        }


        // İşlem kontrolü
        private bool IslemExists(int id)
        {
            return _context.Islemler.Any(e => e.Id == id);
        }
    }
}
