using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Fitness_Center_Web_Project.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        private IActionResult? CheckAdminRole()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return null;

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Randevular()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var randevular = await _context.Randevular
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Personel)
                .Include(r => r.Islem)
                .OrderByDescending(r => r.RandevuTarihi)
                .ThenByDescending(r => r.RandevuSaati)
                .ToListAsync();

            return View(randevular);
        }

        // Randevu Onaylama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = "Onaylandı";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Randevular));
        }

        // Randevu Reddetme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = "Reddedildi";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Randevular));
        }

        // Randevu Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Randevu başarıyla silindi.";
            return RedirectToAction(nameof(Randevular));
        }

        // Kazanç Listeleme (REST API üzerinden)
        [HttpGet]
        public async Task<IActionResult> KazancListele()
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            // Not: API portu projene göre değişebilir. Çalışmazsa bunu AppSettings'ten alacak hale getiririz.
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7001/api/Kazanc/AylikKazanclar");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Kazanç bilgisi alınamadı (API yanıt vermedi).";
                return View(new List<Kazanc>());
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var kazancListesi = JsonSerializer.Deserialize<List<Kazanc>>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Kazanc>();

            return View(kazancListesi);
        }
    }
}
