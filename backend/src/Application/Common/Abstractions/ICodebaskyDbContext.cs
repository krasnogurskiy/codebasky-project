using Codebasky.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.Common.Abstractions;

public interface ICodebaskyDbContext
{
    DbSet<Workspace> Workspaces { get; }

    DbSet<WorkspaceMember> WorkspaceMembers { get; }

    DbSet<Project> Projects { get; }

    DbSet<TaskItem> Tasks { get; }

    DbSet<TaskComment> TaskComments { get; }

    DbSet<TaskActivity> TaskActivities { get; }

    DbSet<NotificationItem> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
