using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using FluentAssertions;

namespace Codebasky.Domain.UnitTests;

public class SupportingEntityTests
{
    [Fact]
    public void WorkspaceMember_ChangeRole_Should_Update_Role()
    {
        var member = new WorkspaceMember(Guid.NewGuid(), "user-1", "Andrii", WorkspaceRole.Guest);

        member.ChangeRole(WorkspaceRole.Member);

        member.Role.Should().Be(WorkspaceRole.Member);
    }

    [Fact]
    public void NotificationItem_MarkAsRead_Should_Set_Flag()
    {
        var notification = new NotificationItem("user-1", NotificationType.Comment, "Title", "Message", Guid.NewGuid());

        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    public void TaskComment_Should_Throw_When_Body_Missing()
    {
        var action = () => new TaskComment(Guid.NewGuid(), "user-1", "Andrii", " ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TaskActivity_Should_Throw_When_Message_Missing()
    {
        var action = () => new TaskActivity(Guid.NewGuid(), "Andrii", " ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotificationItem_Should_Store_Task_Reference()
    {
        var taskId = Guid.NewGuid();

        var notification = new NotificationItem("user-1", NotificationType.Assignment, "Assigned", "Task assigned", taskId);

        notification.TaskItemId.Should().Be(taskId);
        notification.Type.Should().Be(NotificationType.Assignment);
    }
}
