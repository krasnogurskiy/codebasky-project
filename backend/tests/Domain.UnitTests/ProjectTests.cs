using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using FluentAssertions;

namespace Codebasky.Domain.UnitTests;

public class ProjectTests
{
    [Fact]
    public void Rename_Should_Trim_Name_And_Summary()
    {
        var project = new Project(Guid.NewGuid(), "Old", "Old summary");

        project.Rename("  New title  ", "  New summary  ");

        project.Name.Should().Be("New title");
        project.Summary.Should().Be("New summary");
    }

    [Fact]
    public void Rename_Should_Throw_When_Name_Missing()
    {
        var project = new Project(Guid.NewGuid(), "Old", "Old summary");

        var action = () => project.Rename(" ", "summary");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangeStatus_Should_Update_Project_Status()
    {
        var project = new Project(Guid.NewGuid(), "Old", "Old summary");

        project.ChangeStatus(ProjectStatus.OnHold);

        project.Status.Should().Be(ProjectStatus.OnHold);
    }

    [Fact]
    public void AddTask_Should_Create_Task_With_Default_Status()
    {
        var project = new Project(Guid.NewGuid(), "Old", "Old summary");

        var task = project.AddTask("Task", "Desc", "user-1", "Andrii", DateTime.UtcNow, WorkItemPriority.High, "FR-01");

        task.Title.Should().Be("Task");
        task.Status.Should().Be(WorkItemStatus.Todo);
        project.Tasks.Should().ContainSingle();
    }
}
