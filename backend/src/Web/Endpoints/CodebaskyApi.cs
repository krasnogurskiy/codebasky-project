using Codebasky.Application.Models;
using Codebasky.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace Codebasky.Web.Endpoints;

public static class CodebaskyApi
{
    public static IEndpointRouteBuilder MapCodebaskyApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/session", [Authorize("WorkspaceRead")] async (SessionService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetSessionAsync(cancellationToken)));

        api.MapGet("/workspaces/current", [Authorize("WorkspaceRead")] async (WorkspaceService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetCurrentWorkspaceAsync(cancellationToken)));

        api.MapGet("/projects", [Authorize("WorkspaceRead")] async (Guid workspaceId, ProjectService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(workspaceId, cancellationToken)));

        api.MapPost("/projects", [Authorize("ManagerOnly")] async (CreateProjectRequest request, ProjectService service, CancellationToken cancellationToken) =>
        {
            var project = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/projects/{project.Id}", project);
        });

        api.MapGet("/tasks", [Authorize("WorkspaceRead")] async (Guid? projectId, string? assignee, TaskService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(projectId, assignee, cancellationToken)));

        api.MapGet("/tasks/{taskId:guid}", [Authorize("WorkspaceRead")] async (Guid taskId, TaskService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAsync(taskId, cancellationToken)));

        api.MapPost("/tasks", [Authorize("MemberWrite")] async (CreateTaskRequest request, TaskService service, CancellationToken cancellationToken) =>
        {
            var task = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/tasks/{task.Id}", task);
        });

        api.MapPut("/tasks/{taskId:guid}", [Authorize("MemberWrite")] async (Guid taskId, UpdateTaskRequest request, TaskService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAsync(taskId, request, cancellationToken)));

        api.MapGet("/tasks/{taskId:guid}/comments", [Authorize("WorkspaceRead")] async (Guid taskId, CommentService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(taskId, cancellationToken)));

        api.MapPost("/tasks/{taskId:guid}/comments", [Authorize("MemberWrite")] async (Guid taskId, AddCommentRequest request, CommentService service, CancellationToken cancellationToken) =>
        {
            var comment = await service.AddAsync(taskId, request, cancellationToken);
            return Results.Created($"/api/tasks/{taskId}/comments/{comment.Id}", comment);
        });

        api.MapGet("/analytics", [Authorize("ManagerOnly")] async (AnalyticsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAsync(cancellationToken)));

        api.MapGet("/notifications", [Authorize("WorkspaceRead")] async (NotificationService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(cancellationToken)));

        api.MapPost("/notifications/{notificationId:guid}/read", [Authorize("WorkspaceRead")] async (Guid notificationId, NotificationService service, CancellationToken cancellationToken) =>
        {
            await service.MarkAsReadAsync(notificationId, cancellationToken);
            return Results.NoContent();
        });

        return app;
    }
}
