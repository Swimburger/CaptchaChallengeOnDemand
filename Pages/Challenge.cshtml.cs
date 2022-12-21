using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CaptchaChallengeOnDemand.Pages;

public class ChallengeModel : PageModel
{
    private readonly ILogger<ChallengeModel> logger;
    private readonly HttpClient httpClient;
    private readonly string recaptchaSecret;

    public string RecaptchaSiteKey { get; init; }
    public string? ProtectedChallenge { get; private set; }

    public ChallengeModel(
        ILogger<ChallengeModel> logger,
        HttpClient httpClient,
        IConfiguration config
    )
    {
        this.logger = logger;
        this.httpClient = httpClient;
        recaptchaSecret = config["Recaptcha:Secret"] ?? throw new Exception("'Recaptcha:Secret' not configured.");
        RecaptchaSiteKey = config["Recaptcha:SiteKey"] ?? throw new Exception("'Recaptcha:SiteKey' not configured.");
    }

    public void OnGet()
    {
    }

    public async Task OnPost(string challenge, [FromServices] ChallengeResultProtector challengeResultProtector)
    {
        if (!ModelState.IsValid)
        {
            return;
        }

        var form = await Request.ReadFormAsync().ConfigureAwait(false);
        var captchaResponse = form["g-recaptcha-response"];
        if (string.IsNullOrEmpty(captchaResponse))
        {
            ModelState.AddModelError("g-recaptcha-response-missing", "The reCaptcha was not submitted.");
            return;
        }

        var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"secret", recaptchaSecret},
                    {"response", captchaResponse},
                    {"remoteip", Request.HttpContext.Connection.RemoteIpAddress.ToString()}
                }))
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>()
            .ConfigureAwait(false);
        var jsonObject = responseBody.RootElement;
        var success = jsonObject.GetProperty("success").GetBoolean();
        var timeStamp = jsonObject.GetProperty("challenge_ts").GetDateTimeOffset();
        if (!success)
        {
            ModelState.AddModelError("g-recaptcha-response-incorrect", "The reCaptcha was not correct.");
            return;
        }

        var unprotectedChallenge = challengeResultProtector.Unprotect(challenge);
        unprotectedChallenge.Solved = success;
        unprotectedChallenge.TimeStamp = timeStamp;

        ProtectedChallenge = challengeResultProtector.Protect(unprotectedChallenge);
    }
}