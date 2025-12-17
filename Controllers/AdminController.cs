using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Fitness_Center_Web_Project.Context;
using Fitness_Center_Web_Project.Models;

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

        public IActionResult AdminDashboard()
        {
            // Session'dan rolü al
            var role = HttpContext.Session.GetString("Role");

            // Eğer kullanıcı admin değilse, erişim reddedilir
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        public async Task<IActionResult> Randevular()
        {
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var randevular = await _context.Randevular
                .Include(r => r.User)
                .Include(r => r.Personel)
                .Include(r => r.Islem)
                .ToListAsync();

            return View(randevular);
        }

        // Randevu Onaylama
        [HttpPost]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            randevu.Durum = "Onaylandı";
            await _context.SaveChangesAsync();

            return RedirectToAction("Randevular");
        }

        // Randevu Reddetme
        [HttpPost]
        public async Task<IActionResult> Reddet(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            // _context.Randevular.Remove(randevu); 
            randevu.Durum = "Reddedildi";
            await _context.SaveChangesAsync();

            return RedirectToAction("Randevular");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Randevu başarıyla silindi.";
            return RedirectToAction("Randevular", "Admin"); // Admin panelindeki randevu listesini gösteren action
        }

        public async Task<IActionResult> KazancListele()
        {
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7001/api/Kazanc/AylikKazanclar"); // API rotası

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Kazanç bilgisi alınamadı.";
                return View();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var kazancListesi = JsonSerializer.Deserialize<List<Kazanc>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(kazancListesi); // Model olarak kazanç listesini gönderiyoruz
        }
    }
}
