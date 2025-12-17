using System.ComponentModel.DataAnnotations;

namespace Fitness_Center_Web_Project.Models
{
    public class UserLogin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Sifre { get; set; }
    }
}
