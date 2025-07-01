using System.ComponentModel.DataAnnotations;

namespace Grad_Project_Dashboard_1.Models
{
    public class CustomRule
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string Method { get; set; }

        [Required]
        [StringLength(50)]
        public string Location { get; set; }

        [Required]
        public string Regex { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    
        public ICollection<UserCustomRule> UserCustomRules { get; set; }
    }
}
