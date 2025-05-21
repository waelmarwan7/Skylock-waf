using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Grad_Project_Dashboard_1.Models;
using System.Text.Json;
//using WafDashboard.Models;

namespace Grad_Project_Dashboard_1.Services

{
    public class RequestLogsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RequestLogsService> _logger;

        public RequestLogsService(HttpClient httpClient, ILogger<RequestLogsService> logger)
        {
            _httpClient = httpClient;
           
            _logger = logger;
        }

public async Task<List<RequestLogsClass>> GetLogsFromFile(string endpoint)
{
    try
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Host = "34.72.112.1";
    //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        // Deserializing the JSON array into a list of your model
        var logs = JsonSerializer.Deserialize<List<RequestLogsClass>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return logs ?? new List<RequestLogsClass>();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to fetch or parse log entries");
        return new List<RequestLogsClass>();
    }
}

    }
}