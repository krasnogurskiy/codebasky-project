using Codebasky.Domain.Common;
using Codebasky.Domain.Enums;

namespace Codebasky.Domain.Entities;

public class WorkspaceMember : AuditableEntity
{
    private WorkspaceMember()
    {
    }

    public WorkspaceMember(Guid workspaceId, string userId, string displayName, WorkspaceRole role)
    {
        WorkspaceId = workspaceId;
        UserId = string.IsNullOrWhiteSpace(userId) ? throw new ArgumentException("User id is required.", nameof(userId)) : userId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? throw new ArgumentException("Display name is required.", nameof(displayName)) : displayName.Trim();
        Role = role;
    }

    public Guid WorkspaceId { get; private set; }

    public Workspace Workspace { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public WorkspaceRole Role { get; private set; }

    public void ChangeRole(WorkspaceRole role)
    {
        Role = role;
        Touch();
    }
}
