using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Center_Web_Project.Models
{
    public class PersonelUzmanlik
    {
        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage = "Uzmanlık adı zorunludur.")]
        public string? UzmanlikAdi { get; set; }

        public ICollection<Islem> Islemler { get; set; } = new List<Islem>();

        public ICollection<Personel> Personeller { get; set; } = new List<Personel>();

    }

}
