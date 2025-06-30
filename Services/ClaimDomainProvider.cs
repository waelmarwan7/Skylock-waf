// Add this new file to your Services folder
public interface IDomainProvider
{
    string GetCurrentDomain();
}

public class ClaimDomainProvider : IDomainProvider
{
    private readonly HttpContextAccessor _httpContextAccessor;

    public ClaimDomainProvider()
    {
        _httpContextAccessor = new HttpContextAccessor();
    }

    public string GetCurrentDomain()
    {
        // Try to get IPInstance first, fall back to IPAddress
        var domain = _httpContextAccessor.HttpContext?.User.FindFirst("IPInstance")?.Value;

        if (string.IsNullOrEmpty(domain))
            throw new InvalidOperationException("No domain available for current user");
            
        return domain;
    }
}