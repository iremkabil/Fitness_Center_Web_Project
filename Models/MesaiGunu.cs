using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Center_Web_Project.Models
{
    // MesaiGunu = Mesai kaydının hangi günlerde geçerli olduğunu tutar
    public class MesaiGunu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MesaiId { get; set; }

        [ForeignKey(nameof(MesaiId))]
        [ValidateNever]
        public Mesai Mesai { get; set; } = default!;

        [Required]
        public DayOfWeek Gun { get; set; }
    }
}
