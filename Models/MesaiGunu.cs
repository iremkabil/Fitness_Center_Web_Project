using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Fitness_Center_Web_Project.Models
{
    public class MesaiGunu
    {
        [Key]
        public int Id { get; set; }


        [Required]
        public int? MesaiId { get; set; }

        [ForeignKey("MesaiId")]
        [ValidateNever]
        public Mesai? Mesai { get; set; }

        public DayOfWeek Gun { get; set; }

    }
}
