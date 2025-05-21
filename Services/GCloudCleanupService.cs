using Google.Apis.Auth.OAuth2;
using Google.Apis.Compute.v1;
using Google.Apis.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace Grad_Project_Dashboard_1.Services
{
    public class GCloudCleanupService
    {
        private string projectId = "modified-fabric-457423-r4";
        private string region = "us-central1";
        private string zone = "us-central1-a";
        private ComputeService computeService;

        public GCloudCleanupService(IConfiguration config)
        {
            // Same initialization as GCloudManager
            string credentialsPath = config["GoogleCloud:CredentialsPath"] ?? "service-account.json";
            string fullPath = Path.Combine(AppContext.BaseDirectory, credentialsPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Google Cloud credentials file not found at: {fullPath}");
            }

            GoogleCredential credential;
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(ComputeService.Scope.Compute);
            }

            computeService = new ComputeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "InfraDeployer",
            });
        }

        public async Task CleanupInfrastructure(string userName)
        {
            try
            {
                string networkName = $"{userName}-network";
                string instanceGroupName = $"{userName}-instance-group";
                string lbName = $"{userName}-load-balancer";
                string ipName = $"{userName}-static-ip";

                Console.WriteLine($"Starting cleanup for {userName}...");
                
                // Delete resources in reverse order of creation with retries
                await SafeDelete(() => DeleteLoadBalancer(lbName, ipName), "Load Balancer Resources");
                await SafeDelete(() => DeleteInstanceGroup(instanceGroupName, networkName), "Instance Group");
                await SafeDelete(() => DeleteNetwork(networkName), "Network");

                Console.WriteLine("Cleanup completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup failed: {ex.Message}");
                throw;
            }
        }

        private async Task SafeDelete(Func<Task> deleteAction, string resourceType)
        {
            int retries = 3;
            while (retries-- > 0)
            {
                try
                {
                    await deleteAction();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {3 - retries} failed for {resourceType}: {ex.Message}");
                    if (retries > 0)
                    {
                        await Task.Delay(5000); // Wait 5 seconds before retrying
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async Task DeleteLoadBalancer(string lbName, string ipName)
        {
            // 1. Remove forwarding rule first
            await TryExecuteCommand($"compute forwarding-rules delete {lbName}-forwarding-rule --global --quiet");
            
            // 2. Remove target proxy
            await TryExecuteCommand($"compute target-https-proxies delete {lbName}-target-proxy --quiet");
            
            // 3. Remove URL map
            await TryExecuteCommand($"compute url-maps delete {lbName} --quiet");
            
            try 
            {
                // 4. Try to remove backend from backend service (but don't fail if it doesn't exist)
                await TryExecuteCommand($"compute backend-services remove-backend {lbName}-backend-service " +
                                    $"--instance-group={lbName.Replace("-load-balancer", "-instance-group")} " +
                                    $"--instance-group-zone={zone} --global --quiet");
                // 4. Try to remove backend2 from backend service (but don't fail if it doesn't exist)
                await TryExecuteCommand($"compute backend-services remove-backend {lbName}-api-backend-service " +
                                    $"--instance-group={lbName.Replace("-load-balancer", "-instance-group")} " +
                                    $"--instance-group-zone={zone} --global --quiet");
            }
            catch 
            {
                Console.WriteLine("Backend removal failed (likely never added), proceeding with deletion");
            }
            
            // 5. Delete backend service
            await TryExecuteCommand($"compute backend-services delete {lbName}-backend-service --global --quiet");
            await TryExecuteCommand($"compute backend-services delete {lbName}-api-backend-service --global --quiet");
            
            // 6. Delete IP address
            await TryExecuteCommand($"compute addresses delete {ipName} --global --quiet");
            
            // 7. Delete LB-specific firewall rules
            await TryExecuteCommand($"compute firewall-rules delete allow-lb-{lbName.Replace("-load-balancer", "")} --quiet");
            await TryExecuteCommand($"compute firewall-rules delete deny-all-{lbName.Replace("-load-balancer", "")} --quiet");
        }

        private async Task DeleteInstanceGroup(string instanceGroupName, string networkName)
        {
            try
            {
                // WAF rule deletion.
                await TryExecuteCommand($"compute firewall-rules delete allow-waf-{instanceGroupName} --quiet");

                // 1. Check if autoscaling is enabled
                var checkAutoscalerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c gcloud compute instance-groups managed describe {instanceGroupName} --zone={zone} --format=\"value(autoscaler)\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                checkAutoscalerProcess.Start();
                string autoscalerOutput = await checkAutoscalerProcess.StandardOutput.ReadToEndAsync();
                await checkAutoscalerProcess.WaitForExitAsync();
                
                // Check if autoscaler exists
                if (!string.IsNullOrWhiteSpace(autoscalerOutput))
                {
                    await TryExecuteCommand($"compute instance-groups managed stop-autoscaling {instanceGroupName} --zone={zone} --quiet");
                }

                // 2. Delete instance group
                await TryExecuteCommand($"compute instance-groups managed delete {instanceGroupName} --zone={zone} --quiet");

                // 3. Delete instance template 
                string templateName = $"{instanceGroupName}-template";
                await TryExecuteCommand($"compute instance-templates delete {templateName} --quiet");
                
                
                // 4. Delete subnet
                await TryExecuteCommand($"compute networks subnets delete {networkName}-subnet --region={region} --quiet");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning during instance group cleanup: {ex.Message}");
                throw;
            }
        }
        private async Task DeleteNetwork(string networkName)
        {
            try 
            {
                // 1. First delete all firewall rules (including any LB-specific ones)
                var allFirewallRules = await ListFirewallRules();
                var rulesToDelete = allFirewallRules
                    .Where(rule => rule.Contains(networkName) || 
                        rule.Contains($"allow-lb-{networkName.Replace("-network", "")}"))
                    .ToList();

                foreach (var rule in rulesToDelete)
                {
                    await TryExecuteCommand($"compute firewall-rules delete {rule} --quiet");
                    await Task.Delay(2000); // Short delay between deletions
                }

                // 2. Then delete the network
                await TryExecuteCommand($"compute networks delete {networkName} --quiet");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning during network cleanup: {ex.Message}");
                throw;
            }
        }

        private async Task<List<string>> ListFirewallRules()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c gcloud compute firewall-rules list --format=\"value(name)\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private async Task TryExecuteCommand(string command)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c gcloud {command}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Directory.GetCurrentDirectory()
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(output))
                    Console.WriteLine(output);
                
                if (process.ExitCode != 0)
                {
                    // Special case for network deletion that might need manual intervention
                    if (command.Contains("networks delete") && 
                        error.Contains("already being used by"))
                    {
                        Console.WriteLine($"Network still in use. Remaining dependencies:\n{error}");
                        throw new Exception($"Network has remaining dependencies. Manual cleanup may be needed.\n{error}");
                    }
                    
                    if (!error.Contains("was not found") && !error.Contains("already deleted"))
                    {
                        throw new Exception($"Command failed: {error}");
                    }
                    Console.WriteLine($"Resource may be already deleted: {command.Split(' ')[2]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning during command execution: {ex.Message}");
                throw;
            }
        }
    }
}