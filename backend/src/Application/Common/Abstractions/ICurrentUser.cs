using Codebasky.Domain.Enums;

namespace Codebasky.Application.Common.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string UserId { get; }

    string DisplayName { get; }

    WorkspaceRole Role { get; }
}
