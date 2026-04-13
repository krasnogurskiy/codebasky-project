using Codebasky.Application.Common.Abstractions;
using Codebasky.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.Services;

public sealed class NotificationService(ICodebaskyDbContext dbContext, ICurrentUser currentUser)
{
    public async Task<IReadOnlyCollection<NotificationDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == currentUser.UserId)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .Select(notification => new NotificationDto(
                notification.Id,
                notification.Type,
                notification.Title,
                notification.Message,
                notification.TaskItemId,
                notification.IsRead,
                notification.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(item => item.Id == notificationId && item.UserId == currentUser.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Notification not found.");

        notification.MarkAsRead();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
