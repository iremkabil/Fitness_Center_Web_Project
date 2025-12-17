using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Center_Web_Project.Models
{
    // Islem = Fitness tarafında "Hizmet/Seans" gibi düşüneceğiz (şimdilik sınıf adı aynı kalsın).
    public class Islem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Hizmet adı 100 karakterden uzun olamaz.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Ücret sıfırdan küçük olamaz.")]
        public decimal Ucret { get; set; }

        // Örn: 00:45:00 (45 dk) gibi
        [Required(ErrorMessage = "Süre zorunludur.")]
        public TimeSpan Sure { get; set; }

        // Bu ilişkiyi şimdilik koruyoruz; ilerleyen adımda PersonelUzmanlik -> TrainerExpertise'a çevireceğiz.
        public int? UzmanlikId { get; set; }
        public PersonelUzmanlik? Uzmanlik { get; set; }

        // Fitness için faydalı ek alanlar (UI/filtre için)
        [StringLength(300)]
        public string? Aciklama { get; set; }

        public bool AktifMi { get; set; } = true;
    }
}
