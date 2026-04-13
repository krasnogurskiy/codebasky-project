using Codebasky.Domain.Common;
using Codebasky.Domain.Enums;

namespace Codebasky.Domain.Entities;

public class TaskItem : AuditableEntity
{
    private readonly List<TaskComment> _comments = [];
    private readonly List<TaskActivity> _activities = [];

    private TaskItem()
    {
    }

    public TaskItem(
        Guid projectId,
        string title,
        string description,
        string? assigneeUserId,
        string? assigneeDisplayName,
        DateTime? dueDateUtc,
        WorkItemPriority priority,
        string? requirementKey)
    {
        ProjectId = projectId;
        Title = NormalizeRequired(title, nameof(title));
        Description = description.Trim();
        AssigneeUserId = NormalizeOptional(assigneeUserId);
        AssigneeDisplayName = NormalizeOptional(assigneeDisplayName);
        DueDateUtc = dueDateUtc;
        Priority = priority;
        RequirementKey = NormalizeOptional(requirementKey);
        Status = WorkItemStatus.Todo;
    }

    public Guid ProjectId { get; private set; }

    public Project Project { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public WorkItemStatus Status { get; private set; }

    public WorkItemPriority Priority { get; private set; }

    public string? AssigneeUserId { get; private set; }

    public string? AssigneeDisplayName { get; private set; }

    public DateTime? DueDateUtc { get; private set; }

    public string? RequirementKey { get; private set; }

    public IReadOnlyCollection<TaskComment> Comments => _comments;

    public IReadOnlyCollection<TaskActivity> Activities => _activities;

    public bool IsOverdue(DateTime nowUtc)
    {
        return DueDateUtc.HasValue && DueDateUtc.Value.Date < nowUtc.Date && Status != WorkItemStatus.Done;
    }

    public void UpdateDetails(
        string title,
        string description,
        string? assigneeUserId,
        string? assigneeDisplayName,
        DateTime? dueDateUtc,
        WorkItemPriority priority,
        string? requirementKey)
    {
        Title = NormalizeRequired(title, nameof(title));
        Description = description.Trim();
        AssigneeUserId = NormalizeOptional(assigneeUserId);
        AssigneeDisplayName = NormalizeOptional(assigneeDisplayName);
        DueDateUtc = dueDateUtc;
        Priority = priority;
        RequirementKey = NormalizeOptional(requirementKey);
        Touch();
    }

    public void ChangeStatus(WorkItemStatus status)
    {
        if (Status == WorkItemStatus.Done && status == WorkItemStatus.Todo)
        {
            throw new InvalidOperationException("A completed task cannot move back to To Do directly.");
        }

        Status = status;
        Touch();
    }

    public void Assign(string assigneeUserId, string assigneeDisplayName)
    {
        AssigneeUserId = NormalizeRequired(assigneeUserId, nameof(assigneeUserId));
        AssigneeDisplayName = NormalizeRequired(assigneeDisplayName, nameof(assigneeDisplayName));
        Touch();
    }

    public TaskComment AddComment(string authorUserId, string authorDisplayName, string body)
    {
        var comment = new TaskComment(Id, authorUserId, authorDisplayName, body);
        _comments.Add(comment);
        Touch();
        return comment;
    }

    public TaskActivity AddActivity(string actorDisplayName, string message)
    {
        var activity = new TaskActivity(Id, actorDisplayName, message);
        _activities.Add(activity);
        Touch();
        return activity;
    }

    private static string NormalizeRequired(string value, string argumentName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", argumentName)
            : value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
