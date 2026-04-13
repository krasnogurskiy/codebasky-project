using Codebasky.Application.Common.Abstractions;
using Codebasky.Application.Models;
using Codebasky.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.Services;

public sealed class ProjectService(ICodebaskyDbContext dbContext)
{
    public async Task<IReadOnlyCollection<ProjectSummaryDto>> ListAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.WorkspaceId == workspaceId)
            .OrderBy(project => project.Name)
            .ToListAsync(cancellationToken);

        var projectIds = projects.Select(project => project.Id).ToArray();
        var tasks = await dbContext.Tasks
            .AsNoTracking()
            .Where(task => projectIds.Contains(task.ProjectId))
            .ToListAsync(cancellationToken);

        return projects
            .Select(project => new ProjectSummaryDto(
                project.Id,
                project.Name,
                project.Summary,
                project.Status,
                tasks.Count(task => task.ProjectId == project.Id && task.Status != WorkItemStatus.Done)))
            .ToArray();
    }

    public async Task<ProjectSummaryDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var workspace = await dbContext.Workspaces
            .Include(item => item.Projects)
            .FirstOrDefaultAsync(item => item.Id == request.WorkspaceId, cancellationToken)
            ?? throw new InvalidOperationException("Workspace not found.");

        var project = workspace.AddProject(request.Name, request.Summary);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectSummaryDto(project.Id, project.Name, project.Summary, project.Status, 0);
    }
}
