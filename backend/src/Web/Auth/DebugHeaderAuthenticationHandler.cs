using System.Security.Claims;
using System.Text.Encodings.Web;
using Codebasky.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Codebasky.Web.Auth;

public sealed class DebugHeaderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "DebugHeaders";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-Debug-UserId"].FirstOrDefault();
        var displayName = Request.Headers["X-Debug-UserName"].FirstOrDefault();
        var role = Request.Headers["X-Debug-Role"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Enum.TryParse<WorkspaceRole>(role, ignoreCase: true, out var parsedRole))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid role header."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Role, parsedRole.ToString()),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
