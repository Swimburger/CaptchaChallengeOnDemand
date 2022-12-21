using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CaptchaChallengeOnDemand.Pages;

public class ChallengeViewerModel : PageModel
{
    private readonly ILogger<ChallengeViewerModel> logger;
    private readonly ChallengeResultProtector challengeResultProtector;

    [BindProperty, Required] public string? ProtectedChallenge { get; set; }
    public Challenge? ChallengeResult { get; private set; }
    public Guid? ChallengeId { get; private set; }
    public bool ShowTable { get; private set; }

    public ChallengeViewerModel(
        ILogger<ChallengeViewerModel> logger,
        ChallengeResultProtector challengeResultProtector
    )
    {
        this.logger = logger;
        this.challengeResultProtector = challengeResultProtector;
    }

    public void OnGet()
    {
    }

    public void OnPost(Guid id)
    {
        if (!ModelState.IsValid)
        {
            return;
        }

        try
        {
            ChallengeResult = challengeResultProtector.Unprotect(ProtectedChallenge);
            ChallengeId = id;
            ShowTable = true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An unexpected error occurred while unprotecting the challenge.");
            ModelState.AddModelError("error-unprotecting-challenge",
                "Could not decrypt challenge. Make sure the code is correct and complete.");
        }
    }
}