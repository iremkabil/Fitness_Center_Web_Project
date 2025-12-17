using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Center_Web_Project.Models
{
    // Kuaför projesindeki "saç önerisi" yerine Fitness için:
    // Kullanıcı bilgilerine göre AI ile "antrenman/diyet önerisi" üretme form modeli gibi kullanacağız.
    public class AiOneri
    {
        [Key]
        public int Id { get; set; }

        // Fotoğraf opsiyonel kalsın (istersen ileride kullanırsın)
        [NotMapped]
        [Display(Name = "Fotoğraf (Opsiyonel)")]
        [DataType(DataType.Upload)]
        public IFormFile? Photo { get; set; }

        // Fitness input alanları
        [Required(ErrorMessage = "Boy zorunludur.")]
        [Range(120, 230, ErrorMessage = "Boy 120-230 cm aralığında olmalıdır.")]
        public int BoyCm { get; set; }

        [Required(ErrorMessage = "Kilo zorunludur.")]
        [Range(30, 250, ErrorMessage = "Kilo 30-250 kg aralığında olmalıdır.")]
        public int KiloKg { get; set; }

        [Required(ErrorMessage = "Yaş zorunludur.")]
        [Range(10, 80, ErrorMessage = "Yaş 10-80 aralığında olmalıdır.")]
        public int Yas { get; set; }

        [Required(ErrorMessage = "Hedef seçiniz.")]
        [StringLength(30)]
        public string Hedef { get; set; } = "Kilo Verme"; // Kilo Verme / Kas Kazanma / Form Koruma

        [Required(ErrorMessage = "Aktivite seviyesi seçiniz.")]
        [StringLength(30)]
        public string AktiviteSeviyesi { get; set; } = "Orta"; // Düşük / Orta / Yüksek

        // AI çıktısı (DB’de saklanacaksa mapped olmalı)
        [StringLength(2000)]
        public string? OneriMetni { get; set; }

        // UI dropdownları için (NotMapped)
        [NotMapped]
        public List<string> Hedefler { get; set; } = new()
        {
            "Kilo Verme",
            "Kas Kazanma",
            "Form Koruma"
        };

        [NotMapped]
        public List<string> AktiviteSeviyeleri { get; set; } = new()
        {
            "Düşük",
            "Orta",
            "Yüksek"
        };
    }
}
// Not: Bu değişiklik büyük
// (saç enum listesini kaldırıp fitness form alanları ekledik).
// Eğer mevcut AI sayfan/Controller’ı saç tipine göre yazılmışsa,
// o controller/view da güncellenmeden bu ekran çalışmaz;
// ama model tarafını fitness’a çevirmek için doğru adım.