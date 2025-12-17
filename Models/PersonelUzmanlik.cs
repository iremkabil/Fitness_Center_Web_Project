using System.ComponentModel.DataAnnotations;

namespace Fitness_Center_Web_Project.Models
{
    // PersonelUzmanlik = Fitness tarafında "Uzmanlık/Branş" (Yoga, Pilates, PT vb.)
    public class PersonelUzmanlik
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Uzmanlık adı zorunludur.")]
        [StringLength(80, ErrorMessage = "Uzmanlık adı 80 karakterden uzun olamaz.")]
        public string UzmanlikAdi { get; set; } = string.Empty;

        // Bu uzmanlığa bağlı verilebilecek hizmet/seanslar
        public ICollection<Islem> Islemler { get; set; } = new List<Islem>();

        // Bu uzmanlığa sahip antrenörler
        public ICollection<Personel> Personeller { get; set; } = new List<Personel>();

        // Opsiyonel: UI’da aktif/pasif
        public bool AktifMi { get; set; } = true;
    }
}

