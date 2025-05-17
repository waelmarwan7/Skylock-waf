
using System.ComponentModel.DataAnnotations;

namespace Grad_Project_Dashboard_1.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Delete my account after login")]
        public bool DeleteAfterLogin { get; set; }
    }
}