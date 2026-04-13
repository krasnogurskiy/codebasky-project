using Codebasky.Application.Common.Abstractions;
using Codebasky.Application.Models;
using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.Services;

public sealed class TaskService(
    ICodebaskyDbContext dbContext,
    ICurrentUser currentUser,
    IRealtimeNotifier realtimeNotifier)
{
    public async Task<IReadOnlyCollection<TaskSummaryDto>> ListAsync(Guid? projectId, string? assigneeFilter, CancellationToken cancellationToken)
    {
        var query = dbContext.Tasks
            .AsNoTracking()
            .Include(task => task.Project)
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(task => task.ProjectId == projectId.Value);
        }

        if (string.Equals(assigneeFilter, "mine", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(task => task.AssigneeUserId == currentUser.UserId);
        }

        var tasks = await query
            .OrderBy(task => task.Status)
            .ThenByDescending(task => task.Priority)
            .ThenBy(task => task.Title)
            .ToListAsync(cancellationToken);

        return tasks.Select(MapSummary).ToArray();
    }

    public async Task<TaskDetailsDto> GetAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks
            .AsNoTracking()
            .Include(item => item.Project)
            .Include(item => item.Comments)
            .Include(item => item.Activities)
            .FirstOrDefaultAsync(item => item.Id == taskId, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        return MapDetails(task);
    }

    public async Task<TaskSummaryDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .Include(item => item.Tasks)
            .FirstOrDefaultAsync(item => item.Id == request.ProjectId, cancellationToken)
            ?? throw new InvalidOperationException("Project not found.");

        var task = project.AddTask(
            request.Title,
            request.Description,
            request.AssigneeUserId,
            request.AssigneeDisplayName,
            request.DueDateUtc,
            request.Priority,
            request.RequirementKey);

        dbContext.Tasks.Add(task);
        var activity = task.AddActivity(currentUser.DisplayName, "Task created");
        dbContext.TaskActivities.Add(activity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await realtimeNotifier.PublishTaskChangedAsync(project.WorkspaceId, task.Id, cancellationToken);

        return await GetSummaryAsync(task.Id, cancellationToken);
    }

    public async Task<TaskSummaryDto> UpdateAsync(Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks
            .Include(item => item.Project)
            .Include(item => item.Activities)
            .FirstOrDefaultAsync(item => item.Id == taskId, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        var oldStatus = task.Status;
        task.UpdateDetails(
            request.Title,
            request.Description,
            request.AssigneeUserId,
            request.AssigneeDisplayName,
            request.DueDateUtc,
            request.Priority,
            request.RequirementKey);

        if (task.Status != request.Status)
        {
            task.ChangeStatus(request.Status);
        }

        if (!string.Equals(task.AssigneeUserId, request.AssigneeUserId, StringComparison.Ordinal))
        {
            if (!string.IsNullOrWhiteSpace(request.AssigneeUserId) && !string.IsNullOrWhiteSpace(request.AssigneeDisplayName))
            {
                task.Assign(request.AssigneeUserId, request.AssigneeDisplayName);
            }
        }

        var activity = oldStatus != request.Status
            ? task.AddActivity(currentUser.DisplayName, $"Status updated to {request.Status}.")
            : task.AddActivity(currentUser.DisplayName, "Task details updated.");

        dbContext.TaskActivities.Add(activity);

        await dbContext.SaveChangesAsync(cancellationToken);
        await realtimeNotifier.PublishTaskChangedAsync(task.Project.WorkspaceId, task.Id, cancellationToken);

        return await GetSummaryAsync(taskId, cancellationToken);
    }

    private async Task<TaskSummaryDto> GetSummaryAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks
            .AsNoTracking()
            .Include(item => item.Project)
            .FirstAsync(item => item.Id == taskId, cancellationToken);

        return MapSummary(task);
    }

    private static TaskSummaryDto MapSummary(TaskItem task)
    {
        return new TaskSummaryDto(
            task.Id,
            task.ProjectId,
            task.Project.Name,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssigneeUserId,
            task.AssigneeDisplayName,
            task.DueDateUtc,
            task.RequirementKey,
            task.IsOverdue(DateTime.UtcNow),
            task.UpdatedAtUtc);
    }

    private static TaskDetailsDto MapDetails(TaskItem task)
    {
        return new TaskDetailsDto(
            task.Id,
            task.ProjectId,
            task.Project.Name,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssigneeUserId,
            task.AssigneeDisplayName,
            task.DueDateUtc,
            task.RequirementKey,
            task.Activities
                .OrderByDescending(item => item.CreatedAtUtc)
                .Select(item => new TaskActivityDto(item.Id, item.ActorDisplayName, item.Message, item.CreatedAtUtc))
                .ToArray(),
            task.Comments
                .OrderBy(item => item.CreatedAtUtc)
                .Select(item => new TaskCommentDto(item.Id, item.AuthorDisplayName, item.Body, item.CreatedAtUtc))
                .ToArray());
    }
}
