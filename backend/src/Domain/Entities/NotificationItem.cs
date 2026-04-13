using Codebasky.Domain.Common;
using Codebasky.Domain.Enums;

namespace Codebasky.Domain.Entities;

public class NotificationItem : AuditableEntity
{
    private NotificationItem()
    {
    }

    public NotificationItem(string userId, NotificationType type, string title, string message, Guid? taskItemId)
    {
        UserId = string.IsNullOrWhiteSpace(userId) ? throw new ArgumentException("User id is required.", nameof(userId)) : userId.Trim();
        Type = type;
        Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Title is required.", nameof(title)) : title.Trim();
        Message = string.IsNullOrWhiteSpace(message) ? throw new ArgumentException("Message is required.", nameof(message)) : message.Trim();
        TaskItemId = taskItemId;
    }

    public string UserId { get; private set; } = string.Empty;

    public NotificationType Type { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public Guid? TaskItemId { get; private set; }

    public bool IsRead { get; private set; }

    public void MarkAsRead()
    {
        IsRead = true;
        Touch();
    }
}
