
// using System.ComponentModel.DataAnnotations;
// namespace Grad_Project_Dashboard_1.Models
// {
//     public class User
//     {
//         public int Id { get; set; }

//         [Required]
//         [Display(Name = "Username")]
//         public string Username { get; set; }

//         [Required]
//         [EmailAddress]
//         public string Email { get; set; }

//         [Required]
//         [DataType(DataType.Password)]
//         public string Password { get; set; }
//         [Required]
//         public string IPAddress { get; set; }
//         public string DomainName { get; set; }
//         // Add this new property
//         public string IPInstance { get; set; } = "null-ip";  // LoadBalancerIp
//         public string? InstanceGroupName { get; set; }
//         public string? LoadBalancerName { get; set; }
//         public string? NetworkName { get; set; }

//         public ICollection<UserCustomRule> UserCustomRules { get; set; }
//     }

// }

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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
        // Navigation property for domain names
        public bool AIToggle { get; set; } = true; // Default to true (enabled)
        public ICollection<UserDomainName> DomainNames { get; set; }
        
        public string IPInstance { get; set; } = "null-ip";  // LoadBalancerIp
        public string? InstanceGroupName { get; set; }
        public string? LoadBalancerName { get; set; }
        public string? NetworkName { get; set; }

        public ICollection<UserCustomRule> UserCustomRules { get; set; }
    }

    public class UserDomainName
    {
        public int Id { get; set; }
        
        [Required]
        public string Domain { get; set; }
        public string IPAddress { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
    }
}