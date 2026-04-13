using Codebasky.Domain.Common;

namespace Codebasky.Domain.Entities;

public class TaskComment : AuditableEntity
{
    private TaskComment()
    {
    }

    public TaskComment(Guid taskItemId, string authorUserId, string authorDisplayName, string body)
    {
        TaskItemId = taskItemId;
        AuthorUserId = string.IsNullOrWhiteSpace(authorUserId) ? throw new ArgumentException("Author id is required.", nameof(authorUserId)) : authorUserId.Trim();
        AuthorDisplayName = string.IsNullOrWhiteSpace(authorDisplayName) ? throw new ArgumentException("Author name is required.", nameof(authorDisplayName)) : authorDisplayName.Trim();
        Body = string.IsNullOrWhiteSpace(body) ? throw new ArgumentException("Comment body is required.", nameof(body)) : body.Trim();
    }

    public Guid TaskItemId { get; private set; }

    public TaskItem TaskItem { get; private set; } = null!;

    public string AuthorUserId { get; private set; } = string.Empty;

    public string AuthorDisplayName { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;
}
