using Codebasky.Domain.Enums;

namespace Codebasky.Application.Models;

public sealed record ProjectSummaryDto(
    Guid Id,
    string Name,
    string Summary,
    ProjectStatus Status,
    int OpenTasks);

public sealed record CreateProjectRequest(
    Guid WorkspaceId,
    string Name,
    string Summary);
