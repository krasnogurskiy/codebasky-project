using Codebasky.Domain.Enums;

namespace Codebasky.Application.Models;

public sealed record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    Guid? TaskItemId,
    bool IsRead,
    DateTime CreatedAtUtc);
