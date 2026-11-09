using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace RaceDay.Api.Middleware;

/// <summary>
/// Authenticates API requests using the ASP.NET Core session established by AuthController.Login.
/// The API session cookie is forwarded by the MVC application's typed API client.
/// </summary>
public sealed class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "RaceDaySession";

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Context.Session.GetInt32("UserID");
        var roleName = Context.Session.GetString("RoleName");
        if (!userId.HasValue || string.IsNullOrWhiteSpace(roleName))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(ClaimTypes.Name, Context.Session.GetString("Email") ?? string.Empty)
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
