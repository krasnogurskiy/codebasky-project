using Codebasky.Application.Services;
using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using FluentAssertions;

namespace Codebasky.Application.UnitTests;

public sealed class AnalyticsServiceTests
{
    [Fact]
    public async Task GetAsync_returns_dashboard_counts_risks_and_overdue_focus()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var workspace = new Workspace("Codebasky Semester Team", "Semester delivery workspace");
        var project = new Project(workspace.Id, "Codebasky MVP", "Core MVP delivery");

        var doneTask = new TaskItem(project.Id, "Finished task", "Already done.", "user-lead", "Капарис Андрій", DateTime.UtcNow.AddDays(-1), WorkItemPriority.Medium, "FR-01");
        doneTask.ChangeStatus(WorkItemStatus.InProgress);
        doneTask.ChangeStatus(WorkItemStatus.Done);

        var overdueTask = new TaskItem(project.Id, "Overdue critical task", "Needs manager attention.", "user-backend", "Богдан", DateTime.UtcNow.AddDays(-2), WorkItemPriority.Critical, "NFR-02");
        overdueTask.ChangeStatus(WorkItemStatus.InProgress);

        var openTask = new TaskItem(project.Id, "Open high priority task", "Still in progress.", "user-manager", "Красногурський Андрій", DateTime.UtcNow.AddDays(3), WorkItemPriority.High, "FR-03");
        openTask.ChangeStatus(WorkItemStatus.InProgress);

        dbContext.Workspaces.Add(workspace);
        dbContext.Projects.Add(project);
        dbContext.Tasks.AddRange(doneTask, overdueTask, openTask);
        await dbContext.SaveChangesAsync();

        var service = new AnalyticsService(dbContext);

        var analytics = await service.GetAsync(CancellationToken.None);

        analytics.TotalTasks.Should().Be(3);
        analytics.DoneThisSprint.Should().Be(1);
        analytics.InProgress.Should().Be(2);
        analytics.Overdue.Should().Be(1);
        analytics.Throughput.Should().HaveCount(4);
        analytics.Risks.Should().Contain(item => item.Title.Contains("Overdue tasks"));
        analytics.Risks.Should().Contain(item => item.Title.Contains("High priority work"));
        analytics.OverdueFocus.Should().NotBeNull();
        analytics.OverdueFocus!.Title.Should().Be("Overdue critical task");
        analytics.OverdueFocus.Owner.Should().Be("Богдан");
        analytics.OverdueFocus.RequirementKey.Should().Be("NFR-02");
    }
}
