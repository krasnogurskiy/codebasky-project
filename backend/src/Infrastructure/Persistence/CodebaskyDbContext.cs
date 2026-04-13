using Codebasky.Application.Common.Abstractions;
using Codebasky.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Infrastructure.Persistence;

public sealed class CodebaskyDbContext(DbContextOptions<CodebaskyDbContext> options)
    : DbContext(options), ICodebaskyDbContext
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();

    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<TaskComment> TaskComments => Set<TaskComment>();

    public DbSet<TaskActivity> TaskActivities => Set<TaskActivity>();

    public DbSet<NotificationItem> Notifications => Set<NotificationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.ToTable("Workspaces");
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(1000);
            entity.HasMany(item => item.Members).WithOne(item => item.Workspace).HasForeignKey(item => item.WorkspaceId);
            entity.HasMany(item => item.Projects).WithOne(item => item.Workspace).HasForeignKey(item => item.WorkspaceId);
        });

        modelBuilder.Entity<WorkspaceMember>(entity =>
        {
            entity.ToTable("WorkspaceMembers");
            entity.HasIndex(item => new { item.WorkspaceId, item.UserId }).IsUnique();
            entity.Property(item => item.UserId).HasMaxLength(100).IsRequired();
            entity.Property(item => item.DisplayName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Summary).HasMaxLength(1000);
            entity.HasMany(item => item.Tasks).WithOne(item => item.Project).HasForeignKey(item => item.ProjectId);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.Property(item => item.Title).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(4000);
            entity.Property(item => item.AssigneeUserId).HasMaxLength(100);
            entity.Property(item => item.AssigneeDisplayName).HasMaxLength(200);
            entity.Property(item => item.RequirementKey).HasMaxLength(50);
            entity.HasMany(item => item.Comments).WithOne(item => item.TaskItem).HasForeignKey(item => item.TaskItemId);
            entity.HasMany(item => item.Activities).WithOne(item => item.TaskItem).HasForeignKey(item => item.TaskItemId);
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.ToTable("TaskComments");
            entity.Property(item => item.AuthorUserId).HasMaxLength(100).IsRequired();
            entity.Property(item => item.AuthorDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Body).HasMaxLength(2000).IsRequired();
        });

        modelBuilder.Entity<TaskActivity>(entity =>
        {
            entity.ToTable("TaskActivities");
            entity.Property(item => item.ActorDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Message).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<NotificationItem>(entity =>
        {
            entity.ToTable("Notifications");
            entity.Property(item => item.UserId).HasMaxLength(100).IsRequired();
            entity.Property(item => item.Title).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Message).HasMaxLength(1000).IsRequired();
        });
    }
}
