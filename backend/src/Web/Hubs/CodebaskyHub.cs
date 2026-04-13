using Microsoft.AspNetCore.SignalR;

namespace Codebasky.Web.Hubs;

public sealed class CodebaskyHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var workspaceId = Context.GetHttpContext()?.Request.Query["workspaceId"].FirstOrDefault();
        var userId = Context.GetHttpContext()?.Request.Query["userId"].FirstOrDefault();

        if (Guid.TryParse(workspaceId, out var parsedWorkspaceId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetWorkspaceGroupName(parsedWorkspaceId));
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
        }

        await base.OnConnectedAsync();
    }

    public static string GetWorkspaceGroupName(Guid workspaceId) => $"workspace:{workspaceId}";

    public static string GetUserGroupName(string userId) => $"user:{userId}";
}
