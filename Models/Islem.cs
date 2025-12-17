using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Center_Web_Project.Models
{
    public class Islem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "İşlem adı zorunludur.")]
        [StringLength(100, ErrorMessage = "İşlem adı 100 karakterden uzun olamaz.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Ucret zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Ücret sıfırdan küçük olamaz.")]
        public decimal Ucret { get; set; }

        [Required(ErrorMessage = "Süre zorunludur.")]
        public TimeSpan Sure { get; set; }



        [ForeignKey("PersonelUzmanlik")]
        public int? UzmanlikId { get; set; } = 0;
        public PersonelUzmanlik? Uzmanlik { get; set; } = default!;
    }

}

