namespace Codebasky.Application.Common.Abstractions;

public interface IRealtimeNotifier
{
    Task PublishTaskChangedAsync(Guid workspaceId, Guid taskId, CancellationToken cancellationToken);

    Task PublishCommentAddedAsync(Guid workspaceId, Guid taskId, CancellationToken cancellationToken);

    Task PublishNotificationAsync(string userId, CancellationToken cancellationToken);
}
