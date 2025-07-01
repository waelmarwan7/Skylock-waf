

namespace Grad_Project_Dashboard_1.Models{
public class GeoIPControlsClass
{
    public string RuleName { get; set; } = string.Empty;

    // Comma-separated list of IP addresses (e.g., "192.168.1.1,203.0.113.5")
    public string IPs { get; set; } = string.Empty;

    // Comma-separated list of country names or ISO codes
    public string Countries { get; set; } = string.Empty;

    // Optional priority value
    public int? Priority { get; set; }

    // Enabled state (default true)
    public bool Enabled { get; set; } = true;
}


}