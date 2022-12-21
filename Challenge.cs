using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace CaptchaChallengeOnDemand;

public class ChallengeResultProtector
{
    private readonly IDataProtector protector;

    public ChallengeResultProtector(IDataProtectionProvider provider)
    {
        protector = provider.CreateProtector(nameof(ChallengeResultProtector));
    }

    public string Protect(Challenge result)
    {
        var json =  JsonSerializer.Serialize(result);
        var protectedJson = protector.Protect(json);
        return protectedJson;
    }

    public Challenge Unprotect(string protectedJson)
    {
        var unprotectedJson = protector.Unprotect(protectedJson);
        var result =  JsonSerializer.Deserialize<Challenge>(unprotectedJson);
        return result;
    }
}

public class Challenge
{
    public Guid Id { get; set; }
    public bool Solved { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
}