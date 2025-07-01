using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grad_Project_Dashboard_1.Models;

namespace Grad_Project_Dashboard_1.Services
{
    public interface IExternalApiService
    {
        Task<bool> SyncUserWithExternalApi(User user, List<string> domains);
    }

    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;

        public ExternalApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> SyncUserWithExternalApi(User user, List<string> domains)
        {
            var payload = new
            {
                userId = user.Id,
                username = user.Username,
                email = user.Email,
                instanceDetails = new
                {
                    ipInstance = user.IPInstance,
                    instanceGroupName = user.InstanceGroupName,
                    loadBalancerName = user.LoadBalancerName,
                    networkName = user.NetworkName
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("api/users", content);
            return response.IsSuccessStatusCode;
        }
    }
}

                // ipAddress = user.IPAddress,
                // domains = domains,