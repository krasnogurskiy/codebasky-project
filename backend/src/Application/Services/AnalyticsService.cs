using Codebasky.Application.Common.Abstractions;
using Codebasky.Application.Models;
using Codebasky.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Codebasky.Application.Services;

public sealed class AnalyticsService(ICodebaskyDbContext dbContext)
{
    public async Task<AnalyticsDto> GetAsync(CancellationToken cancellationToken)
    {
        var tasks = await dbContext.Tasks
            .AsNoTracking()
            .OrderBy(task => task.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var doneThisSprint = tasks.Count(task => task.Status == WorkItemStatus.Done);
        var inProgress = tasks.Count(task => task.Status == WorkItemStatus.InProgress);
        var overdue = tasks.Count(task => task.IsOverdue(DateTime.UtcNow));
        var lastFourWeeks = Enumerable.Range(0, 4)
            .Select(offset =>
            {
                var start = DateTime.UtcNow.Date.AddDays(-7 * (3 - offset));
                var end = start.AddDays(6);
                return new AnalyticsBarDto(
                    $"W{offset + 1}",
                    tasks.Count(task =>
                        task.Status == WorkItemStatus.Done &&
                        task.UpdatedAtUtc.Date >= start &&
                        task.UpdatedAtUtc.Date <= end));
            })
            .ToArray();

        var risks = new List<RiskItemDto>();
        if (overdue > 0)
        {
            risks.Add(new RiskItemDto("Overdue tasks require attention", $"{overdue} tasks are overdue."));
        }

        if (tasks.Any(task => task.Priority is WorkItemPriority.High or WorkItemPriority.Critical && task.Status != WorkItemStatus.Done))
        {
            risks.Add(new RiskItemDto("High priority work is still open", "Critical backlog items should be reviewed before demo."));
        }

        risks.Add(new RiskItemDto("Keep API and board states aligned", "Realtime and dashboard views should stay consistent across updates."));

        var overdueFocus = tasks
            .Where(task => task.IsOverdue(DateTime.UtcNow))
            .OrderByDescending(task => task.Priority)
            .ThenBy(task => task.DueDateUtc)
            .Select(task => new OverdueFocusDto(task.Id, task.Title, task.AssigneeDisplayName, task.RequirementKey))
            .FirstOrDefault();

        return new AnalyticsDto(tasks.Count, doneThisSprint, inProgress, overdue, lastFourWeeks, risks, overdueFocus);
    }
}
