using Codebasky.Domain.Enums;

namespace Codebasky.Application.Models;

public sealed record WorkspaceOverviewDto(
    Guid WorkspaceId,
    string Name,
    string Description,
    IReadOnlyCollection<WorkspaceMemberDto> Members,
    IReadOnlyCollection<ProjectSummaryDto> Projects,
    int OpenTasks,
    int DueThisWeek);

public sealed record WorkspaceMemberDto(
    string UserId,
    string DisplayName,
    WorkspaceRole Role);
