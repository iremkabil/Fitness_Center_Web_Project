using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Fitness_Center_Web_Project.Models
    {
        public class AiOneriModel
        {
            [Range(10, 90)]
            public int Yas { get; set; }

            [Range(120, 230)]
            public int Boy { get; set; }

            [Range(30, 250)]
            public int Kilo { get; set; }

            [Required]
            public string Cinsiyet { get; set; } = "";

            [Required]
            public string Hedef { get; set; } = "";

            // Kullanıcının yüklediği foto
            public IFormFile? Foto { get; set; }

            // Gemini’den gelen metin plan
            public string? YapayZekaCevabi { get; set; }

            // Üretilen dönüşüm görselinin web yolu (/ai/xxx.png)
            public string? DonusumGorselUrl { get; set; }
        }
    }
