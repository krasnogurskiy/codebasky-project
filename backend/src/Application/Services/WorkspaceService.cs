using Codebasky.Application.Common.Abstractions;
using Codebasky.Application.Models;
using Codebasky.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.Services;

public sealed class WorkspaceService(ICodebaskyDbContext dbContext, ICurrentUser currentUser)
{
    public async Task<WorkspaceOverviewDto> GetCurrentWorkspaceAsync(CancellationToken cancellationToken)
    {
        var membership = await dbContext.WorkspaceMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(member => member.UserId == currentUser.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The current user is not attached to any workspace.");

        var workspace = await dbContext.Workspaces
            .AsNoTracking()
            .Include(item => item.Projects)
            .Include(item => item.Members)
            .FirstAsync(item => item.Id == membership.WorkspaceId, cancellationToken);

        var projectIds = workspace.Projects.Select(project => project.Id).ToArray();
        var tasks = await dbContext.Tasks
            .AsNoTracking()
            .Where(task => projectIds.Contains(task.ProjectId))
            .ToListAsync(cancellationToken);

        var openTasks = tasks.Count(task => task.Status != WorkItemStatus.Done);
        var dueThisWeek = tasks.Count(task =>
            task.DueDateUtc.HasValue &&
            task.DueDateUtc.Value.Date >= DateTime.UtcNow.Date &&
            task.DueDateUtc.Value.Date <= DateTime.UtcNow.Date.AddDays(7));

        return new WorkspaceOverviewDto(
            workspace.Id,
            workspace.Name,
            workspace.Description,
            workspace.Members
                .OrderByDescending(member => member.Role)
                .Select(member => new WorkspaceMemberDto(member.UserId, member.DisplayName, member.Role))
                .ToArray(),
            workspace.Projects
                .OrderBy(project => project.Name)
                .Select(project => new ProjectSummaryDto(
                    project.Id,
                    project.Name,
                    project.Summary,
                    project.Status,
                    tasks.Count(task => task.ProjectId == project.Id && task.Status != WorkItemStatus.Done)))
                .ToArray(),
            openTasks,
            dueThisWeek);
    }
}
