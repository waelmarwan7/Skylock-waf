namespace Grad_Project_Dashboard_1.Models
{
    public class RateLimitingClass
    {
        public int RequestsPerMinute { get; set; }
        public int BurstCapacity { get; set; }
    }
}
