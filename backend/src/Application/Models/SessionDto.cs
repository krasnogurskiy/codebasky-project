using Codebasky.Domain.Enums;

namespace Codebasky.Application.Models;

public sealed record SessionDto(
    string UserId,
    string DisplayName,
    WorkspaceRole Role,
    Guid WorkspaceId,
    string WorkspaceName);
