using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CaptchaChallengeOnDemand.Pages;

public class ChallengeCreatorModel : PageModel
{
    private readonly ILogger<ChallengeCreatorModel> logger;

    public ChallengeCreatorModel(
        ILogger<ChallengeCreatorModel> logger
    )
    {
        this.logger = logger;
    }

    public string? ChallengeUrl { get; set; }
    public Guid? ChallengeId { get; set; }


    public void OnGet()
    {
    }

    public void OnPost([FromServices] ChallengeResultProtector challengeResultProtector)
    {
        if (!ModelState.IsValid)
        {
            return;
        }

        var challenge = new Challenge
        {
            Id = Guid.NewGuid()
        };
        ChallengeId = challenge.Id;

        var protectedChallenge = challengeResultProtector.Protect(challenge);
        ChallengeUrl = Url.Page(
            pageName: "Challenge",
            pageHandler: null,
            values: new
            {
                challenge = protectedChallenge
            },
            protocol: Request.Scheme,
            host: Request.Host.ToString()
        );
    }
}