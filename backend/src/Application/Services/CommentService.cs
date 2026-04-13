using Codebasky.Application.Common.Abstractions;
using Codebasky.Application.Models;
using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.Services;

public sealed class CommentService(
    ICodebaskyDbContext dbContext,
    ICurrentUser currentUser,
    IRealtimeNotifier realtimeNotifier)
{
    public async Task<IReadOnlyCollection<TaskCommentDto>> ListAsync(Guid taskId, CancellationToken cancellationToken)
    {
        return await dbContext.TaskComments
            .AsNoTracking()
            .Where(comment => comment.TaskItemId == taskId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .Select(comment => new TaskCommentDto(comment.Id, comment.AuthorDisplayName, comment.Body, comment.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<TaskCommentDto> AddAsync(Guid taskId, AddCommentRequest request, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks
            .Include(item => item.Project)
            .Include(item => item.Comments)
            .Include(item => item.Activities)
            .FirstOrDefaultAsync(item => item.Id == taskId, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        var comment = task.AddComment(currentUser.UserId, currentUser.DisplayName, request.Body);
        var activity = task.AddActivity(currentUser.DisplayName, "Comment added.");
        dbContext.TaskComments.Add(comment);
        dbContext.TaskActivities.Add(activity);

        if (!string.IsNullOrWhiteSpace(task.AssigneeUserId) && task.AssigneeUserId != currentUser.UserId)
        {
            dbContext.Notifications.Add(new NotificationItem(
                task.AssigneeUserId,
                NotificationType.Comment,
                $"New comment on {task.Title}",
                $"{currentUser.DisplayName} added a new comment.",
                task.Id));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await realtimeNotifier.PublishCommentAddedAsync(task.Project.WorkspaceId, task.Id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(task.AssigneeUserId) && task.AssigneeUserId != currentUser.UserId)
        {
            await realtimeNotifier.PublishNotificationAsync(task.AssigneeUserId, cancellationToken);
        }

        return new TaskCommentDto(comment.Id, comment.AuthorDisplayName, comment.Body, comment.CreatedAtUtc);
    }
}
