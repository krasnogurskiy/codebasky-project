using System.Security.Claims;
using Codebasky.Application.Common.Abstractions;
using Codebasky.Domain.Enums;

namespace Codebasky.Web.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string UserId => GetRequiredClaim(ClaimTypes.NameIdentifier);

    public string DisplayName => GetRequiredClaim(ClaimTypes.Name);

    public WorkspaceRole Role => Enum.TryParse<WorkspaceRole>(GetRequiredClaim(ClaimTypes.Role), out var role)
        ? role
        : WorkspaceRole.Guest;

    private string GetRequiredClaim(string claimType)
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(claimType)
            ?? throw new InvalidOperationException($"Missing required claim '{claimType}'.");
    }
}
