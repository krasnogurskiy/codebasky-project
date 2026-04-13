namespace Codebasky.Application.Models;

public sealed record AnalyticsDto(
    int TotalTasks,
    int DoneThisSprint,
    int InProgress,
    int Overdue,
    IReadOnlyCollection<AnalyticsBarDto> Throughput,
    IReadOnlyCollection<RiskItemDto> Risks,
    OverdueFocusDto? OverdueFocus);

public sealed record AnalyticsBarDto(string Label, int Value);

public sealed record RiskItemDto(string Title, string Detail);

public sealed record OverdueFocusDto(
    Guid TaskId,
    string Title,
    string? Owner,
    string? RequirementKey);
