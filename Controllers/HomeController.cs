using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitness_Center_Web_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Ana sayfa
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Menüdeki sayfalar (þimdilik basit sayfalar; view'larý ekleyeceðiz)
        [HttpGet]
        public IActionResult Services()
        {
            return View();
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // Ýstersen Privacy kalsýn; yoksa menüden kaldýrýrsýn
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        public IActionResult Error(int? statusCode = null, string? message = null)
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode,
                Message = message
            };

            return View(model);
        }
    }
}
