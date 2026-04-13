using Codebasky.Domain.Common;

namespace Codebasky.Domain.Entities;

public class TaskActivity : AuditableEntity
{
    private TaskActivity()
    {
    }

    public TaskActivity(Guid taskItemId, string actorDisplayName, string message)
    {
        TaskItemId = taskItemId;
        ActorDisplayName = string.IsNullOrWhiteSpace(actorDisplayName) ? throw new ArgumentException("Actor name is required.", nameof(actorDisplayName)) : actorDisplayName.Trim();
        Message = string.IsNullOrWhiteSpace(message) ? throw new ArgumentException("Message is required.", nameof(message)) : message.Trim();
    }

    public Guid TaskItemId { get; private set; }

    public TaskItem TaskItem { get; private set; } = null!;

    public string ActorDisplayName { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;
}
