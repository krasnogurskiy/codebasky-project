using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using FluentAssertions;

namespace Codebasky.Domain.UnitTests;

public class TaskItemTests
{
    [Fact]
    public void UpdateDetails_Should_Update_All_Editable_Fields()
    {
        var task = CreateTask();

        task.UpdateDetails("Updated", "Updated desc", "user-2", "Bogdan", DateTime.UtcNow.AddDays(2), WorkItemPriority.Critical, "FR-12");

        task.Title.Should().Be("Updated");
        task.Description.Should().Be("Updated desc");
        task.AssigneeUserId.Should().Be("user-2");
        task.AssigneeDisplayName.Should().Be("Bogdan");
        task.Priority.Should().Be(WorkItemPriority.Critical);
        task.RequirementKey.Should().Be("FR-12");
    }

    [Fact]
    public void ChangeStatus_Should_Move_From_Todo_To_InProgress()
    {
        var task = CreateTask();

        task.ChangeStatus(WorkItemStatus.InProgress);

        task.Status.Should().Be(WorkItemStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_Should_Throw_When_Moving_Done_Back_To_Todo()
    {
        var task = CreateTask();
        task.ChangeStatus(WorkItemStatus.InProgress);
        task.ChangeStatus(WorkItemStatus.Done);

        var action = () => task.ChangeStatus(WorkItemStatus.Todo);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Assign_Should_Update_Assignee()
    {
        var task = CreateTask();

        task.Assign("user-99", "QA Lead");

        task.AssigneeUserId.Should().Be("user-99");
        task.AssigneeDisplayName.Should().Be("QA Lead");
    }

    [Fact]
    public void AddComment_Should_Append_Comment()
    {
        var task = CreateTask();

        var comment = task.AddComment("user-1", "Andrii", "Looks good");

        task.Comments.Should().ContainSingle();
        comment.Body.Should().Be("Looks good");
    }

    [Fact]
    public void AddActivity_Should_Append_Activity()
    {
        var task = CreateTask();

        var activity = task.AddActivity("Andrii", "Task created");

        task.Activities.Should().ContainSingle();
        activity.Message.Should().Be("Task created");
    }

    [Fact]
    public void IsOverdue_Should_Return_True_For_Open_Past_Due_Task()
    {
        var task = new TaskItem(Guid.NewGuid(), "Task", "Desc", null, null, DateTime.UtcNow.AddDays(-1), WorkItemPriority.High, null);

        var result = task.IsOverdue(DateTime.UtcNow);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_Should_Return_False_For_Completed_Task()
    {
        var task = new TaskItem(Guid.NewGuid(), "Task", "Desc", null, null, DateTime.UtcNow.AddDays(-1), WorkItemPriority.High, null);
        task.ChangeStatus(WorkItemStatus.InProgress);
        task.ChangeStatus(WorkItemStatus.Done);

        var result = task.IsOverdue(DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Title_Missing()
    {
        var action = () => new TaskItem(Guid.NewGuid(), " ", "Desc", null, null, null, WorkItemPriority.Low, null);

        action.Should().Throw<ArgumentException>();
    }

    private static TaskItem CreateTask()
    {
        return new TaskItem(Guid.NewGuid(), "Task", "Desc", "user-1", "Andrii", DateTime.UtcNow.AddDays(1), WorkItemPriority.High, "FR-01");
    }
}
