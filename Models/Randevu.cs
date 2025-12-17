using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Center_Web_Project.Models
{
    public class Randevu
    {
        [Key]
        public int Id { get; set; }

        // Üye
        [Required]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;

        // Antrenör (Personel)
        [Required]
        public int PersonelId { get; set; }
        [ForeignKey(nameof(PersonelId))]
        public Personel Personel { get; set; } = default!;

        // Hizmet/Seans (Islem)
        [Required]
        public int IslemId { get; set; }
        [ForeignKey(nameof(IslemId))]
        public Islem Islem { get; set; } = default!;

        // Randevu zamanı
        [Required]
        public DateTime RandevuTarihi { get; set; }

        [Required]
        public TimeSpan RandevuSaati { get; set; }

        // Fitness senaryosu: Beklemede / Onaylandı / İptal / Tamamlandı
        [Required]
        [StringLength(20)]
        public string Durum { get; set; } = "Beklemede";

        // Seans süresi ve ücreti (randevu anındaki değerler snapshot olarak saklanır)
        [Required]
        public TimeSpan Sure { get; set; }

        [Required]
        public decimal Ucret { get; set; }

        // Opsiyonel: Not alanı (kullanıcı notu / admin notu)
        [StringLength(300)]
        public string? Not { get; set; }

        // Opsiyonel: Randevunun başlangıç/bitişini hesaplamak için yardımcı özellikler
        [NotMapped]
        public DateTime BaslangicDateTime => RandevuTarihi.Date.Add(RandevuSaati);

        [NotMapped]
        public DateTime BitisDateTime => BaslangicDateTime.Add(Sure);
    }
}
