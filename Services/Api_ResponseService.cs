
// Services/ApiService.cs
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Grad_Project_Dashboard_1.Models;
using Newtonsoft.Json;
using System.Text.Json;

namespace Grad_Project_Dashboard_1.Services
{
    public class Api_ResponseService
    {
        private readonly HttpClient _httpClient;

        // In your service constructor
        private readonly ILogger<Api_ResponseService> _logger;

        public Api_ResponseService(HttpClient httpClient, ILogger<Api_ResponseService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Api_ResponseInfoClass> GetSecurityDataAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "Traffic");

                // Add Host header
                request.Headers.Host = "34.72.112.1";

                // Optional: Add Accept header if needed
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                
               var record = System.Text.Json.JsonSerializer.Deserialize<Api_ResponseInfoClass>(content);
                // Parse the plain text response
                
                return record;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Call Failed: {ex.Message}");
                return new Api_ResponseInfoClass
                {
                    Allowed = 0,
                    Blocked_RE = 0,
                    Blocked_AI = 0
                };
            }





        }
/*
 public async Task<Api_RequestCountClass> GetAllowedRequests()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "Allowed");

                // Add Host header
                request.Headers.Host = "34.72.112.1";

                // Optional: Add Accept header if needed
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {content}");

                // Parse the plain text response
                return getcountrequests(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Call Failed: {ex.Message}");
                return new Api_RequestCountClass
                {
                 count=0
                };
            }




            
        }*/
/*
         public async Task<Api_RequestCountClass> GetBlockedRequestsByAI()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "Blocked-AI");

                // Add Host header
                request.Headers.Host = "34.72.112.1";

                // Optional: Add Accept header if needed
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {content}");

                // Parse the plain text response
                return getcountrequests(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Call Failed: {ex.Message}");
                return new Api_RequestCountClass
                {
                    count=0
                };
            }




            
        }
*/
/*
         public async Task<Api_RequestCountClass> GetBlockedRequestsByRE()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "Blocked-RE");

                // Add Host header
                request.Headers.Host = "34.72.112.1";

                // Optional: Add Accept header if needed
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {content}");

                // Parse the plain text response
               return getcountrequests(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Call Failed: {ex.Message}");
                return new Api_RequestCountClass
                {
                    count=0
                };
            }




            
        }

*/








      
  /*      private Api_ResponseInfoClass ParsePlainTextResponse(string content)
        {
            var result = new Api_ResponseInfoClass();

            // Split by new lines and then by colon
            var lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "Allowed":
                            if (int.TryParse(value, out int allowed)) result.Allowed = allowed;
                            break;
                        case "Blocked-RE":
                            if (int.TryParse(value, out int blockedRe)) result.Blocked_RE = blockedRe;
                            break;
                        case "Blocked-AI":
                            if (int.TryParse(value, out int blockedAi)) result.Blocked_AI = blockedAi;
                            break;
                    }
                }
            }

            return result;
        }
*//*
  private Api_RequestCountClass getcountrequests(string content)
        {
            var result = new Api_RequestCountClass();

         
            result.count=content.Split('\n')
            .Count(line => !string.IsNullOrWhiteSpace(line));

            return result;
        }
*/
    }
}