using Codebasky.Application.Common.Abstractions;
using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.UnitTests;

internal sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), ICodebaskyDbContext
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();

    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<TaskComment> TaskComments => Set<TaskComment>();

    public DbSet<TaskActivity> TaskActivities => Set<TaskActivity>();

    public DbSet<NotificationItem> Notifications => Set<NotificationItem>();
}

internal static class TestDbContextFactory
{
    public static TestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TestDbContext(options);
    }
}

internal sealed class TestCurrentUser(
    string userId,
    string displayName,
    WorkspaceRole role,
    bool isAuthenticated = true) : ICurrentUser
{
    public bool IsAuthenticated { get; } = isAuthenticated;

    public string UserId { get; } = userId;

    public string DisplayName { get; } = displayName;

    public WorkspaceRole Role { get; } = role;
}
