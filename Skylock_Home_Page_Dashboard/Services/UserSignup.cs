using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Grad_Project_Dashboard_1.Services
{
    public class UserSignup
    {
        private readonly GCloudManager _gcloudManager;
        private readonly GCloudCleanupService _cleanupService;
        private readonly ILogger<UserSignup> _logger;

        public UserSignup(
            GCloudManager gcloudManager,
            GCloudCleanupService cleanupService,
            ILogger<UserSignup> logger)
        {
            _gcloudManager = gcloudManager ?? throw new ArgumentNullException(nameof(gcloudManager));
            _cleanupService = cleanupService ?? throw new ArgumentNullException(nameof(cleanupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SignupResult> ProcessSignupAsync(string email , int id)
        {
            string userName = email.Split('@')[0].ToLower();
            string networkName = $"{userName}-network";
            // string ip = ip_address;
            try
            {
                _logger.LogInformation("Starting infrastructure setup for {UserName}", userName);
                
                // Add delay between operations
                Console.WriteLine("before setupinfra");
                await _gcloudManager.SetupInfrastructure(networkName, userName , id);
                await Task.Delay(10000); // Additional delay after setup

                _logger.LogInformation("Infrastructure setup completed for {UserName}", userName);
                return new SignupResult { Success = true, Message = "Infrastructure deployed successfully", UserName = userName };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to setup infrastructure for {UserName}", userName);
                
                // Enhanced cleanup with retries
                int retries = 3;
                while (retries-- > 0)
                {
                    try
                    {
                        await _cleanupService.CleanupInfrastructure(userName);
                        break;
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, $"Cleanup attempt {3-retries} failed for {userName}");
                        if (retries > 0) await Task.Delay(10000);
                    }
                }

                return new SignupResult { Success = false, Message = $"Signup failed: {ex.Message}", UserName = userName };
            }
        }

        public async Task<CleanupResult> CleanupUserResourcesAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Cleanup attempt with empty email");
                throw new ArgumentException("Email is required.");
            }

            string userName = email.Split('@')[0].ToLower();

            try
            {
                _logger.LogInformation("Starting resource cleanup for {UserName}", userName);
                await _cleanupService.CleanupInfrastructure(userName);
                _logger.LogInformation("Resource cleanup completed for {UserName}", userName);
                
                return new CleanupResult
                {
                    Success = true,
                    Message = "Resources cleaned up successfully",
                    UserName = userName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup resources for {UserName}", userName);
                return new CleanupResult
                {
                    Success = false,
                    Message = $"Cleanup failed: {ex.Message}",
                    UserName = userName
                };
            }
        }
    }

    public class SignupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public string NetworkName { get; set; }
    }

    public class CleanupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
    }
}