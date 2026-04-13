using Codebasky.Domain.Enums;

namespace Codebasky.Application.Models;

public sealed record TaskSummaryDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Title,
    string Description,
    WorkItemStatus Status,
    WorkItemPriority Priority,
    string? AssigneeUserId,
    string? AssigneeDisplayName,
    DateTime? DueDateUtc,
    string? RequirementKey,
    bool IsOverdue,
    DateTime UpdatedAtUtc);

public sealed record TaskDetailsDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Title,
    string Description,
    WorkItemStatus Status,
    WorkItemPriority Priority,
    string? AssigneeUserId,
    string? AssigneeDisplayName,
    DateTime? DueDateUtc,
    string? RequirementKey,
    IReadOnlyCollection<TaskActivityDto> Activities,
    IReadOnlyCollection<TaskCommentDto> Comments);

public sealed record TaskActivityDto(
    Guid Id,
    string ActorDisplayName,
    string Message,
    DateTime CreatedAtUtc);

public sealed record TaskCommentDto(
    Guid Id,
    string AuthorDisplayName,
    string Body,
    DateTime CreatedAtUtc);

public sealed record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    string Description,
    string? AssigneeUserId,
    string? AssigneeDisplayName,
    DateTime? DueDateUtc,
    WorkItemPriority Priority,
    string? RequirementKey);

public sealed record UpdateTaskRequest(
    string Title,
    string Description,
    WorkItemStatus Status,
    WorkItemPriority Priority,
    string? AssigneeUserId,
    string? AssigneeDisplayName,
    DateTime? DueDateUtc,
    string? RequirementKey);

public sealed record AddCommentRequest(string Body);
