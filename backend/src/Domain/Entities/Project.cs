using Codebasky.Domain.Common;
using Codebasky.Domain.Enums;

namespace Codebasky.Domain.Entities;

public class Project : AuditableEntity
{
    private readonly List<TaskItem> _tasks = [];

    private Project()
    {
    }

    public Project(Guid workspaceId, string name, string summary)
    {
        WorkspaceId = workspaceId;
        Rename(name, summary);
        Status = ProjectStatus.Active;
    }

    public Guid WorkspaceId { get; private set; }

    public Workspace Workspace { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public ProjectStatus Status { get; private set; }

    public IReadOnlyCollection<TaskItem> Tasks => _tasks;

    public void Rename(string name, string summary)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        Name = name.Trim();
        Summary = summary.Trim();
        Touch();
    }

    public void ChangeStatus(ProjectStatus status)
    {
        Status = status;
        Touch();
    }

    public TaskItem AddTask(
        string title,
        string description,
        string? assigneeUserId,
        string? assigneeDisplayName,
        DateTime? dueDateUtc,
        WorkItemPriority priority,
        string? requirementKey)
    {
        var task = new TaskItem(Id, title, description, assigneeUserId, assigneeDisplayName, dueDateUtc, priority, requirementKey);
        _tasks.Add(task);
        Touch();
        return task;
    }
}
