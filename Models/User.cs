using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Center_Web_Project.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        // TR formatı: +90 ile başlayıp 10 hane (5xxxxxxxxx)
        [RegularExpression(@"^\+90\d{10}$", ErrorMessage = "Telefon numarası +90 ile başlamalı ve 10 haneli olmalıdır.")]
        [StringLength(13)]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(100)]
        public string Sifre { get; set; } = string.Empty;

        // Admin / User
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";

        // UI kolaylığı
        [NotMapped]
        public string AdSoyad => $"{Ad} {Soyad}";
    }
}

// Not: Şifre şu an düz metin olarak tutuluyor
// (mevcut projeyi bozmamak için).
// Sonradan istersen hash’leme ekleyebiliriz,
// ama önce akışı çalışır hale getirelim.