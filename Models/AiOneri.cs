using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProje.Models
{
    public class AiOneri
    {
        [Key] 
        public int Id { get; set; }

        [NotMapped]
        [Required]
        [Display(Name = "Fotoğraf")]
        [DataType(DataType.Upload)]
        public IFormFile? Photo { get; set; }


        [Required]
        [Display(Name = "Saç Tipi")]
        public int? HairStyle { get; set; }

        [NotMapped]
        public Dictionary<int, string> HairStyles { get; set; } = new Dictionary<int, string>
        {
                { 101, "Bangs (default)" },
                { 201, "Long hair" },
                { 301, "Bangs with long hair" },
                { 401, "Medium hair increase" },
                { 402, "Light hair increase" },
                { 403, "Heavy hair increase" },
                { 502, "Light curling" },
                { 503, "Heavy curling" },
                { 603, "Short hair" },
                { 801, "Blonde" },
                { 901, "Straight hair" },
                { 1001, "Oil-free hair" },
                { 1101, "Hairline fill" },
                { 1201, "Smooth hair" },
                { 1301, "Fill hair gap" }
         };

    }
}
