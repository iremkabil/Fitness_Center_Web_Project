using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Fitness_Center_Web_Project.Models
{
    public class Personel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden uzun olamaz.")]
        public string? Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad 50 karakterden uzun olamaz.")]
        public string? Soyad { get; set; }

        [Required]
        [StringLength(10)]
        public string? Cinsiyet { get; set; }

        [ValidateNever]
        public ICollection<PersonelUzmanlik> Uzmanliklar { get; set; } = new List<PersonelUzmanlik>();

        [ValidateNever]
        public ICollection<Mesai> Mesailer { get; set; } = new List<Mesai>();

    }

}
