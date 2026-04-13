using Codebasky.Application.Common.Abstractions;
using Codebasky.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Codebasky.Web.Services;

public sealed class SignalRRealtimeNotifier(IHubContext<CodebaskyHub> hubContext) : IRealtimeNotifier
{
    public Task PublishTaskChangedAsync(Guid workspaceId, Guid taskId, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Group(CodebaskyHub.GetWorkspaceGroupName(workspaceId))
            .SendAsync("taskChanged", new { taskId }, cancellationToken);
    }

    public Task PublishCommentAddedAsync(Guid workspaceId, Guid taskId, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Group(CodebaskyHub.GetWorkspaceGroupName(workspaceId))
            .SendAsync("commentAdded", new { taskId }, cancellationToken);
    }

    public Task PublishNotificationAsync(string userId, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Group(CodebaskyHub.GetUserGroupName(userId))
            .SendAsync("notificationChanged", new { userId }, cancellationToken);
    }
}
