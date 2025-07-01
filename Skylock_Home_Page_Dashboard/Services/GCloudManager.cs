using Google.Apis.Auth.OAuth2;
using Google.Apis.Compute.v1;
using Google.Apis.Services;
using Grad_Project_Dashboard_1.Models;
using System;
using System.Text;
using System.Runtime.InteropServices; // For RuntimeInformation
using System.Diagnostics;
namespace Grad_Project_Dashboard_1.Services;


public class GCloudManager
{
    private string projectId = "modified-fabric-457423-r4";
    private string region = "us-central1";
    private string zone = "us-central1-a";
    private string networkName;
    private string imageName = "final-image";
    private string instanceGroupName = "my-instance-group";
    private string lbName = "my-load-balancer";
    private string ipName = "lb-static-ip";
    private string WafIp = "34.72.112.1/32";
    private string allowedSshIp = "0.0.0.0/0"; // Replace with your actual public IP
    private string lbIp= null;
    private ComputeService computeService;




    public GCloudManager(IConfiguration config)
    {
        // Get credentials path from configuration
        string credentialsPath = config["GoogleCloud:CredentialsPath"] ?? "service-account.json";
        
        // Resolve full path (handles both development and published environments)
        string fullPath = Path.Combine(AppContext.BaseDirectory, credentialsPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Google Cloud credentials file not found at: {fullPath}");
        }

        // Authenticate with Google Cloud
        GoogleCredential credential;
        using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(ComputeService.Scope.Compute);
        }

        // Initialize Compute Service
        computeService = new ComputeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "InfraDeployer",
        });

        // Log initialization (optional)
        Console.WriteLine($"Initialized GCloudManager for project: {projectId}");
    }


    public async Task SetupInfrastructure(string networkName, string userName  , int id)
    {
        try
        {
            this.networkName = networkName;
            instanceGroupName = $"{userName}-instance-group";
            lbName = $"{userName}-load-balancer";
            ipName = $"{userName}-static-ip";
            lbIp= null;
            
            // 1. Create Network
            Console.WriteLine($"Creating network: {networkName}");
            CreateNetwork();
            
            // 2. Create Instance Group
            Console.WriteLine($"Creating instance group from image: {imageName}");
            await CreateInstanceGroupAsync(imageName);
            
            // 3. Create Load Balancer
            Console.WriteLine("Creating load balancer infrastructure");
            await CreateLoadBalancer();

            // 4. Ensure we have the LB IP before saving
            await GetLoadBalancerIp();
      
            // 5. Create Firewall Rule
            Console.WriteLine("Configuring firewall rules");
            await CreateFirewallRule();


            if (string.IsNullOrEmpty(lbIp))
            {
                throw new Exception("Failed to retrieve load balancer IP address");
            }
            
            Console.WriteLine("Infrastructure setup complete!");
            PutLoadBalancerIp_NetworkName(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Setup failed: {ex.Message}");
            throw;
        }
    }

    private void ExecuteCommand(string command)
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
                WorkingDirectory = Directory.GetCurrentDirectory(),
                Environment = 
                {
                    // Use the system PATH
                    ["PATH"] = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) 
                            + ";" 
                            + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)
                }
            }
        };

        process.Start();
        Console.WriteLine(process.StandardOutput.ReadToEnd());
        Console.WriteLine(process.StandardError.ReadToEnd()); // Add error output
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"gcloud failed with exit code {process.ExitCode}");
        }
    }

    private async Task CreateNetwork()
    {
        // 1. Create the custom mode network
        string command = $"compute networks create {networkName} " +
                    $"--project={projectId} " +
                    $"--subnet-mode=custom";
        ExecuteCommand(command);

        await WaitForResource("networks", networkName, isGlobal: true);
        // 2. Add default firewall rules including web traffic
        AddDefaultFirewallRules();
    }
    private void AddDefaultFirewallRules()
    {
        // Rule 1: Allow internal communication
        ExecuteCommand($"compute firewall-rules create {networkName}-allow-internal " +
                    $"--network {networkName} " +
                    $"--allow tcp,udp,icmp " +
                    $"--source-ranges 10.0.0.0/8");

        // Rule 2: Allow SSH (restricted to your IP)
        ExecuteCommand($"compute firewall-rules create {networkName}-allow-ssh " +
                    $"--network {networkName} " +
                    $"--allow tcp:22 " +
                    $"--source-ranges {allowedSshIp}");


        // Rule 4: Allow ICMP (ping)
        ExecuteCommand($"compute firewall-rules create {networkName}-allow-icmp " +
                    $"--network {networkName} " +
                    $"--allow icmp " +
                    $"--source-ranges 0.0.0.0/0");

        // Rule 5: Allow health checks
        ExecuteCommand($"compute firewall-rules create {networkName}-allow-health-checks " +
                    $"--network {networkName} " +
                    $"--allow tcp:80,tcp:443 " +
                    $"--source-ranges 35.191.0.0/16,130.211.0.0/22,209.85.152.0/22,209.85.204.0/22");

        // Rule 6: allow all other ingress traffic
        ExecuteCommand($"compute firewall-rules create {networkName}-allow-all " +
                    $"--network {networkName} " +
                    $"--action allow " +
                    $"--rules all " +
                    $"--source-ranges 0.0.0.0/0 " +
                    $"--priority 500");
    }


    ///////////////////////////// modified ///////////////////////////////////
    private async Task CreateInstanceGroupAsync(string imageName)
{
    // First create a subnet
    string subnetName = $"{networkName}-subnet";
    string subnetCommand = $"compute networks subnets create {subnetName} " +
                        $"--network={networkName} " +
                        $"--region={region} " +
                        $"--range=10.0.0.0/24";
    ExecuteCommand(subnetCommand);
    await WaitForResource("networks subnets", subnetName);

    string templateName = $"{instanceGroupName}-template";

    // Create a temporary file for the startup script
    string tempScriptPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.sh");
    
    try 
    {
        // Write the startup script to a temporary file
        await File.WriteAllTextAsync(tempScriptPath,
            "#!/bin/bash\n\n" +
            "> /usr/local/openresty/nginx/logs/allowed.log\n"+
            "> /usr/local/openresty/nginx/logs/ai_model.log\n"+
            "> /usr/local/openresty/nginx/logs/error.log\n"+
            "> /usr/local/openresty/nginx/logs/rules_engine.log\n"+
            "> /usr/local/openresty/nginx/logs/modsec_audit.log\n"+
            "> /usr/local/openresty/nginx/logs/modsec_parsed.jsonl\n"+
            "> /usr/local/openresty/nginx/logs/top_blocked.json\n"+
            "exit 0");

        // Create instance template with subnet
        string templateCommand = $"compute instance-templates create {instanceGroupName}-template " +
                            $"--project={projectId} " +
                            $"--machine-type=c2d-highcpu-4 " +
                            $"--provisioning-model=SPOT " +
                            $"--tags=http-server,https-server,lb-health-check " +
                            $"--network={networkName} " +
                            $"--subnet=projects/{projectId}/regions/{region}/subnetworks/{subnetName} " +
                            $"--image={imageName} " +
                            $"--metadata-from-file=startup-script={tempScriptPath} " +
                            $"--boot-disk-type=pd-ssd " +
                            $"--region={region}";

        ExecuteCommand(templateCommand);
        await WaitForResource("instance-templates", templateName);

        // Create managed instance group
        string migCommand = $"compute instance-groups managed create {instanceGroupName} " +
                $"--project={projectId} " +
                $"--template={templateName} " +
                $"--size=1 " +
                $"--default-action-on-vm-failure=repair " +  
                $"--health-check=projects/modified-fabric-457423-r4/global/healthChecks/health-check1 " +
                $"--zone={zone}";
        
        ExecuteCommand(migCommand);
        await WaitForResource("instance-groups managed", instanceGroupName);

        // Set named ports
        await SetNamedPortsForInstanceGroup();

        // Configure autoscaling
        string autoscalingCommand = $"compute instance-groups managed set-autoscaling {instanceGroupName} " +
                        $"--target-cpu-utilization=0.8 " +
                        $"--min-num-replicas=1 " +
                        $"--max-num-replicas=4 " + 
                        $"--zone={zone}";
        
        ExecuteCommand(autoscalingCommand);

        await Task.Delay(30000); // Wait for instances
    }
    finally
    {
        // Clean up the temporary file
        if (File.Exists(tempScriptPath))
        {
            try { File.Delete(tempScriptPath); }
            catch { /* Ignore deletion errors */ }
        }
    }
}

    private async Task SetNamedPortsForInstanceGroup()
    {
        // Use zone-based command instead of region-based
        string command = $"compute instance-groups managed set-named-ports {instanceGroupName} " +
                    $"--zone={zone} " +
                    $"--named-ports=http:80,api:9000";
        
        ExecuteCommand(command);
    }


    private async Task CreateLoadBalancer()
    {
        try
        {
            // 1. Create IP address
            ExecuteCommand($"compute addresses create {ipName} --global");
            await WaitForResource("addresses", ipName, isGlobal: true);


            // 2. Create backend service
            ExecuteCommand($"compute backend-services create {lbName}-backend-service " +
                        $"--protocol HTTP " +
                        $"--port-name http " +  // Reference named port
                        $"--health-checks health-check1 " +
                        $"--global " +
                        $"--no-enable-cdn ");
                    await WaitForResource("backend-services", $"{lbName}-backend-service", isGlobal: true);

            // 3. Add backend with 80% max utilization
            ExecuteCommand($"compute backend-services add-backend {lbName}-backend-service " +
                        $"--instance-group={instanceGroupName} " +
                        $"--instance-group-zone={zone} " +
                        $"--global " +
                        $"--balancing-mode=UTILIZATION " +  // Use UTILIZATION-based balancing
                        $"--max-utilization=0.8");         // Set max to 80% (0.8)


            // 4. Create backend service2
            ExecuteCommand($"compute backend-services create {lbName}-api-backend-service " +
                        $"--protocol HTTP " +
                        $"--port-name api " +  // Reference named port
                        $"--health-checks health-check1 " +
                        $"--global " +
                        $"--no-enable-cdn ");
                    await WaitForResource("backend-services", $"{lbName}-api-backend-service", isGlobal: true);

            // 5. Add backend2 with 80% max utilization
            ExecuteCommand($"compute backend-services add-backend {lbName}-api-backend-service " +
                        $"--instance-group={instanceGroupName} " +
                        $"--instance-group-zone={zone} " +
                        $"--global " +
                        $"--balancing-mode=UTILIZATION " +  // Use UTILIZATION-based balancing
                        $"--max-utilization=0.8");         // Set max to 80% (0.8)

            // 6. Create URL map with host-based routing
            ExecuteCommand($"compute url-maps create {lbName} " +
                     $"--default-service {lbName}-backend-service");
            await WaitForResource("url-maps", lbName);

            // 7. Add host rule for API traffic (133.2.2.2 → port 9000)
            ExecuteCommand($"compute url-maps add-path-matcher {lbName} " +
                        $"--path-matcher-name api-matcher " +
                        $"--default-service {lbName}-api-backend-service " +
                        $"--new-hosts 34.72.112.1");

            // 9. Create target proxy
            ExecuteCommand($"compute target-https-proxies create {lbName}-target-proxy " +
                        $"--ssl-certificates=hackmeagain " +
                        $"--global " +
                        $"--url-map {lbName}");
            await WaitForResource("target-https-proxies", $"{lbName}-target-proxy");

            // 10. Create forwarding rule
            ExecuteCommand($"compute forwarding-rules create {lbName}-forwarding-rule " +
                        $"--address {ipName} " +
                        $"--global " +
                        $"--target-https-proxy {lbName}-target-proxy " +
                        $"--ports 443");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Load balancer creation failed: {ex.Message}");
            throw;
        }
    }


    private async Task CreateFirewallRule()
    {
        int retries = 5;
        while (retries-- > 0 && string.IsNullOrEmpty(lbIp))
        {
            if (string.IsNullOrEmpty(lbIp))
            {
                await Task.Delay(5000);
            }
        }
       
        if (string.IsNullOrEmpty(lbIp))
        {
            throw new Exception("Failed to retrieve load balancer IP address");
        }
        
        // First create the ALLOW rule for LB IP
        string allowLbCommand = $"compute firewall-rules create allow-lb-{instanceGroupName} " +
                        $"--network={networkName} " +
                        $"--direction=INGRESS " +
                        $"--priority=500 " +  // Higher priority than the deny rule
                        $"--source-ranges={lbIp} " +
                        $"--target-tags={instanceGroupName} " +
                        $"--allow=tcp:80,udp,icmp";
        
        ExecuteCommand(allowLbCommand);
        // // First create the ALLOW rule for waf IP
        // string allowWafCommand = $"compute firewall-rules create allow-waf-{instanceGroupName} " +
        //                 $"--network={networkName} " +
        //                 $"--direction=INGRESS " +
        //                 $"--priority=500 " +  // Higher priority than the deny rule
        //                 $"--source-ranges={WafIp} " +
        //                 $"--target-tags={instanceGroupName} " +
        //                 $"--allow=tcp:80,udp,icmp";
        
        // ExecuteCommand(allowWafCommand);

        Console.WriteLine($"Firewall rules created. Only allowing traffic from LB IP: {lbIp}");
    }

     private async Task GetLoadBalancerIp()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c gcloud compute addresses describe {ipName} --global --format=value(address)",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    Environment = 
                    {
                        ["PATH"] = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) + 
                                ";" + 
                                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)
                    }
                }
            };

            process.Start();
            string ip = process.StandardOutput.ReadToEnd().Trim();
            string error = process.StandardError.ReadToEnd().Trim();
            process.WaitForExit();
            Console.WriteLine($"LB IP retrieval attempt. IP: {ip}, Error: {error}");

            if (process.ExitCode != 0 || string.IsNullOrEmpty(ip))
            {
                throw new Exception($"Failed to get LB IP: {error}");
            }

            lbIp=ip;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting LB IP: {ex.Message}");
            throw;
        }
    }

    private async Task WaitForResource(string resourceType, string resourceName, bool isGlobal = false)
    {
        int attempts = 0;
        while (attempts++ < 12)
        {
            try
            {
                string command = $"compute {resourceType} describe {resourceName}";
                
                // Add correct scope flags
                if (resourceType == "networks")
                {
                    // Networks don't need --global flag for describe
                }
                else if (resourceType == "instance-groups managed")
                {
                    command += $" --zone={zone}";
                }
                else if (isGlobal)
                {
                    command += " --global";
                }
                else if (resourceType == "networks subnets")
                {
                    command += $" --region={region}";
                }
                
                ExecuteCommand(command);
                return;
            }
            catch (Exception ex) when (attempts < 12)
            {
                await Task.Delay(5000);
            }
        }
        throw new Exception($"Timeout waiting for {resourceType} {resourceName} to be ready");
    }

    public void PutLoadBalancerIp_NetworkName(int id)
    {
        if (string.IsNullOrEmpty(lbIp))
        {
            throw new InvalidOperationException("Load balancer IP is not available");
        }

        try
        {
            using (var context = new AppDbContext())
            {
                    // Assuming you have a User entity with properties for IPInstance, InstanceGroupName, NetworkName, and LoadBalancerName
                    context.Users.FirstOrDefault(u => u.Id == id).IPInstance = lbIp;
                    context.Users.FirstOrDefault(u => u.Id == id).InstanceGroupName = instanceGroupName;
                    context.Users.FirstOrDefault(u => u.Id == id).NetworkName = networkName;
                    context.Users.FirstOrDefault(u => u.Id == id).LoadBalancerName = lbName;

                    context.SaveChanges();
                    Console.WriteLine($"Successfully saved LB IP {lbIp} to database");
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving user: {ex.Message}");
            throw; // Re-throw to ensure caller knows about the failure
        }
    }

    public void EditFirewallRule(string ipSets, string email, string action, string firewallRuleName, string priority)
{
    try
    {
        Console.WriteLine($"Editing firewall rule: {firewallRuleName}");

        // Step 1: Delete the existing rule
        DeleteFirewallRule(firewallRuleName);

        // Step 2: Recreate the rule with new values
        ApplyFirewallRule(ipSets, email, action, firewallRuleName, priority);

        Console.WriteLine($"Successfully edited firewall rule: {firewallRuleName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to edit firewall rule: {ex.Message}");
        throw;
    }
}


public void ApplyFirewallRule(string ipSets, string email, string action, string firewallRuleName, string priority)
    {
        try
        {

            
            // Sanitize and validate IPs
            var ipRanges = ipSets.Split(',')
                                .Select(ip => ip.Trim())
                                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                                .ToList();

            if (ipRanges.Count == 0 || ipRanges.Count > 50)
            {
                throw new ArgumentException("IP sets must contain 1–50 valid IP ranges.");
            }

            string ipList = string.Join(",", ipRanges);
            string userName = email.Split('@')[0].ToLower();
            string target_tags = $"{userName}-instance-group";
            string networkName = $"{userName}-network";
            string actionFlag = action.ToUpper();


            string ruleDescription = $"Custom Rule from {email}";
            string createRuleCmd = $"compute firewall-rules create {firewallRuleName.ToLower()} " +
                        $"--direction=INGRESS " +
                        $"--network=default " +
                        $"--action={actionFlag} " +
                        $"--rules=tcp:80 " +  // ✅ add space here
                        $"--source-ranges={ipList} " +
                        $"--target-tags={target_tags} " +
                        $"--priority={priority} " +
                        $"--description=\"{ruleDescription}\"";

            ExecuteCommand(createRuleCmd);

            Console.WriteLine($"Successfully applied firewall rule '{firewallRuleName}' for IPs: {ipSets}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to apply firewall rule: {ex.Message}");
            throw;
        }
    }



    public void DeleteFirewallRule(string firewallRuleName)
    {
        try
        {
            
            string deleteCmd = $"compute firewall-rules delete {firewallRuleName.ToLower()} --quiet";
            ExecuteCommand(deleteCmd);
            Console.WriteLine($"Deleted firewall rule: {firewallRuleName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete firewall rule: {ex.Message}");
            throw;
        }
    }





}

   