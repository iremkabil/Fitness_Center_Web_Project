using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
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


        public class KazancDto
        {
            public string Ay { get; set; } = "";
            public decimal Kazanc { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> KazancListele(int? year)
        {
            var roleCheck = CheckAdminRole();
            if (roleCheck != null) return roleCheck;

            int yil = year ?? DateTime.Now.Year;

            try
            {
                var client = _httpClientFactory.CreateClient();

                var response = await client.GetAsync($"https://localhost:7001/api/RaporApi/aylik-kazanc?year={yil}");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Kazanç bilgisi alınamadı (API yanıt vermedi). Status: {(int)response.StatusCode}";
                    ViewBag.SelectedYear = yil;
                    return View(new List<Kazanc>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // API anon obje dönüyor: { ay, kazanc } -> modelin propertyleri Ay ve kazanc ile uyumlu.
                var model = JsonSerializer.Deserialize<List<Kazanc>>(json, options) ?? new List<Kazanc>();

                ViewBag.SelectedYear = yil;
                return View(model);
            }
            catch
            {
                ViewBag.Error = "Kazanç bilgisi alınamadı (API yanıt vermedi).";
                ViewBag.SelectedYear = yil;
                return View(new List<Kazanc>());
            }
        }

    }
}
