namespace Grad_Project_Dashboard_1.Models
{
    public class UserCustomRule
    {
            public int UserId { get; set; }
            public User User { get; set; }

            public int CustomRuleId { get; set; }
            public CustomRule CustomRule { get; set; }

            // You can add more fields here if needed, like:
            // public DateTime AssignedDate { get; set; }
    }
}
