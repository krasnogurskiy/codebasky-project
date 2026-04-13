using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Infrastructure.Persistence;

public sealed class CodebaskyDbContextInitialiser(CodebaskyDbContext dbContext)
{
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Workspaces.AnyAsync(cancellationToken))
        {
            return;
        }

        var workspace = new Workspace("Codebasky Semester Team", "Task planning and collaboration workspace for the semester MVP.");
        workspace.AddMember("user-manager", "Красногурський Андрій", WorkspaceRole.Manager);
        workspace.AddMember("user-lead", "Капарис Андрій", WorkspaceRole.Member);
        workspace.AddMember("user-backend", "Богдан", WorkspaceRole.Member);
        workspace.AddMember("user-guest", "Stakeholder Viewer", WorkspaceRole.Guest);

        var project = workspace.AddProject("Codebasky MVP", "Auth, board, comments, analytics and notifications in one workspace.");
        var task1 = project.AddTask(
            "Finalize workspace role model",
            "Review role boundaries between manager, member and guest before demo.",
            "user-manager",
            "Красногурський Андрій",
            DateTime.UtcNow.Date.AddDays(2),
            WorkItemPriority.High,
            "FR-01");
        task1.AddActivity("Капарис Андрій", "Initial role boundaries drafted.");

        var task2 = project.AddTask(
            "Implement notification center",
            "Show assignment, comment and due-date updates in the product shell.",
            "user-lead",
            "Капарис Андрій",
            DateTime.UtcNow.Date.AddDays(1),
            WorkItemPriority.High,
            "FR-12");
        task2.ChangeStatus(WorkItemStatus.InProgress);
        task2.AddActivity("Богдан", "Realtime payload is ready.");
        task2.AddComment("user-backend", "Богдан", "Realtime payload is ready.");
        task2.AddComment("user-manager", "Красногурський Андрій", "Separate mentions from deadlines visually.");

        var task3 = project.AddTask(
            "Prepare manager dashboard widgets",
            "Add open, in-progress and overdue widgets to the analytics page.",
            "user-manager",
            "Красногурський Андрій",
            DateTime.UtcNow.Date.AddDays(-1),
            WorkItemPriority.Medium,
            "FR-13");
        task3.ChangeStatus(WorkItemStatus.Done);
        task3.AddActivity("Капарис Андрій", "Dashboard baseline approved.");

        var task4 = project.AddTask(
            "Finalize backup and restore policy",
            "Document fallback plan for operational recovery.",
            "user-backend",
            "Богдан",
            DateTime.UtcNow.Date.AddDays(-2),
            WorkItemPriority.Critical,
            "NFR-DOC-06");
        task4.AddActivity("Красногурський Андрій", "Needs final review before release.");

        dbContext.Workspaces.Add(workspace);
        dbContext.Notifications.AddRange(
            new NotificationItem("user-lead", NotificationType.Assignment, "Task assigned", "Notification center is assigned to you.", task2.Id),
            new NotificationItem("user-backend", NotificationType.DueSoon, "Task is overdue", "Backup and restore policy is overdue.", task4.Id),
            new NotificationItem("user-manager", NotificationType.StatusChange, "Dashboard completed", "Manager dashboard widgets were moved to done.", task3.Id));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
