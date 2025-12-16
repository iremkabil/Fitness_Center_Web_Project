using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebProje.Models
{
    public class Mesai
    {
        [Key]
        public int Id { get; set; }


        [ValidateNever]
        public int? PersonelId { get; set; }


        [ForeignKey("PersonelId")]
        [ValidateNever]
        public Personel? Personel { get; set; }

        [Required]
        public TimeSpan BaslangicZamani { get; set; }


        [Required]
        public TimeSpan BitisZamani { get; set; }

        public List<MesaiGunu> CalistigiGunler { get; set; } = new List<MesaiGunu>();
    }
}
