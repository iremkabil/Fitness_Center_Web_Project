using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Fitness_Center_Web_Project.Models
{
    // Personel = Fitness tarafında "Antrenör" (Trainer) gibi düşüneceğiz (şimdilik sınıf adı aynı kalsın).
    public class Personel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden uzun olamaz.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad 50 karakterden uzun olamaz.")]
        public string Soyad { get; set; } = string.Empty;

        // Daha güvenli: serbest string yerine enum da yapılabilir, şimdilik string bırakıyoruz
        [Required(ErrorMessage = "Cinsiyet alanı zorunludur.")]
        [StringLength(10)]
        public string Cinsiyet { get; set; } = "Belirtilmedi";

        // Fitness için işe yarayan ek alanlar
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20)]
        public string? Telefon { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Biyografi { get; set; }

        public bool AktifMi { get; set; } = true;

        // Navigation
        [ValidateNever]
        public ICollection<PersonelUzmanlik> Uzmanliklar { get; set; } = new List<PersonelUzmanlik>();

        [ValidateNever]
        public ICollection<Mesai> Mesailer { get; set; } = new List<Mesai>();
    }
}

//Not: Bu dosyada using System.ComponentModel.DataAnnotations.Schema;
//artık gerekli değil.
//Eğer NotMapped kullanacaksan tekrar eklemen gerekir.
//(Şu an kod NotMapped kullandığı için ekledim.)

