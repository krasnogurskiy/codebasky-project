using Codebasky.Application.Common.Abstractions;
using Codebasky.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.Services;

public sealed class SessionService(ICodebaskyDbContext dbContext, ICurrentUser currentUser)
{
    public async Task<SessionDto> GetSessionAsync(CancellationToken cancellationToken)
    {
        var membership = await dbContext.WorkspaceMembers
            .AsNoTracking()
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(member => member.UserId == currentUser.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The current user is not attached to any workspace.");

        return new SessionDto(
            currentUser.UserId,
            currentUser.DisplayName,
            currentUser.Role,
            membership.WorkspaceId,
            membership.Workspace.Name);
    }
}
