namespace Grad_Project_Dashboard_1.Models{
using System.Text.Json.Serialization;

public class RequestLogsClass
{
    [JsonPropertyName("anomaly_score")]
    public string anomaly_score { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string body { get; set; } = string.Empty;

    [JsonPropertyName("ip")]
    public string ip { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string message { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string method { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int status { get; set; }

    [JsonPropertyName("time_stamp")]
    public string time_stamp { get; set; } = string.Empty;

    [JsonPropertyName("uri")]
    public string uri { get; set; } = string.Empty;
}

}