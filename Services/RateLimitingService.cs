// Services/GeoIPControlsService.cs
using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;

using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace Grad_Project_Dashboard_1.Services
{
    public class RateLimitingService 
    {
        private readonly HttpClient _httpClient;

        public RateLimitingService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> SendRateLimitSettingsAsync(RateLimitingClass settings)
        {
            var url = "http://34.72.112.1/Traffic";
// Replace with your actual endpoint

            // Serialize the settings to JSON using System.Text.Json
            var json = JsonSerializer.Serialize(settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Construct the custom request
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            // Set custom headers
            request.Headers.Host = "34.72.112.1";
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(responseBody);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
