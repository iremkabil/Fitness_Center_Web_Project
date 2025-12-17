using System.ComponentModel.DataAnnotations;

namespace Fitness_Center_Web_Project.Models
{
    public class Kazanc
    {
        [Key]
        public int Id { get; set; }

        // Örn: "Ocak 2026" veya "2026-01"
        [Required(ErrorMessage = "Ay bilgisi zorunludur.")]
        [StringLength(20)]
        public string Ay { get; set; } = string.Empty;

        // AppDbContext içinde HasPrecision(18,2) veriyoruz
        [Required]
        public decimal kazanc { get; set; }
    }
}
