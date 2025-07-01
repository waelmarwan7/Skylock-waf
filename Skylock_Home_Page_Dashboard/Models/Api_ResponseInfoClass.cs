using System.Text.Json.Serialization;
namespace Grad_Project_Dashboard_1.Models
{
public class Api_ResponseInfoClass
{
    [JsonPropertyName("Allowed")]
    public int Allowed { get; set; }

    [JsonPropertyName("Blocked_RE")]
    public int Blocked_RE { get; set; }

    [JsonPropertyName("Blocked_AI")]
    public int Blocked_AI { get; set; }
}
}