using System.ComponentModel.DataAnnotations;

namespace Fitness_Center_Web_Project.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Ad { get; set; }

        [Required]
        [StringLength(50)]
        public string? Soyad { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [RegularExpression(@"^\+90\d{10}$", ErrorMessage = "Telefon numarası +90 ile başlamalı ve 10 haneli olmalıdır.")]
        public string? Telefon { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Sifre { get; set; }

        [Required]
        public string Role { get; set; } = "User"; // Varsayılan olarak "User" atanır
    }
}
