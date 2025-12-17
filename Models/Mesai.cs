using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Center_Web_Project.Models
{
    // Mesai = Antrenörün çalıştığı saat aralığı + günler
    public class Mesai
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PersonelId { get; set; }

        [ForeignKey(nameof(PersonelId))]
        [ValidateNever]
        public Personel Personel { get; set; } = default!;

        [Required(ErrorMessage = "Başlangıç zamanı zorunludur.")]
        public TimeSpan BaslangicZamani { get; set; }

        [Required(ErrorMessage = "Bitiş zamanı zorunludur.")]
        public TimeSpan BitisZamani { get; set; }

        // Örn: Pazartesi, Çarşamba...
        public List<MesaiGunu> CalistigiGunler { get; set; } = new List<MesaiGunu>();

        // Basit doğrulama: bitiş > başlangıç
        [NotMapped]
        public bool SaatAraligiGecerliMi => BitisZamani > BaslangicZamani;
    }
}
// Not: NotMapped kullandığım için
// System.ComponentModel.DataAnnotations.Schema zaten var.