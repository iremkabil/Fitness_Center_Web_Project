using System.ComponentModel.DataAnnotations;

namespace Fitness_Center_Web_Project.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        [StringLength(150, ErrorMessage = "E-posta 150 karakterden uzun olamaz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Şifre 100 karakterden uzun olamaz.")]
        public string Sifre { get; set; } = string.Empty;
    }
}

