
using System.ComponentModel.DataAnnotations;
namespace Grad_Project_Dashboard_1.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string IPAddress { get; set; }
        public string DomainName { get; set; }
        // Add this new property
        public string IPInstance { get; set; } = "null-ip";  // LoadBalancerIp
        public string? InstanceGroupName { get; set; }
        public string? LoadBalancerName { get; set; }
        public string? NetworkName { get; set; }

        public ICollection<UserCustomRule> UserCustomRules { get; set; }
    }

}