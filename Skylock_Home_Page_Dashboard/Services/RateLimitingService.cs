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

public RateLimitingService(HttpClient httpClient)
{
    _httpClient = httpClient;
}

        public async Task<bool> SendRateLimitSettingsAsync(RateLimitingClass settings)
        {
            var url = "Rate-Limit";
            // Replace with your actual endpoint
            Console.WriteLine(settings.burst);
Console.WriteLine(settings.rate);
            // Serialize the settings to JSON using System.Text.Json
            var json = JsonSerializer.Serialize(settings);

            Console.WriteLine(json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

                        
            // Construct the custom request
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            Console.WriteLine(request);

            // Set custom headers
            request.Headers.Host = "34.72.112.1";
            //   request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {



                var response = await _httpClient.SendAsync(request);

                Console.WriteLine("some error lagasgagsgasga");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught exception: {ex.Message}");
                return false;
            }
        }
    }
}
