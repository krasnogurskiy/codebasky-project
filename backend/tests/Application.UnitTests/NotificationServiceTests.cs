using Codebasky.Application.Services;
using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using FluentAssertions;

namespace Codebasky.Application.UnitTests;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task ListAsync_returns_only_current_users_notifications_ordered_newest_first()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var olderNotification = new NotificationItem("user-lead", NotificationType.Assignment, "Task assigned", "A task was assigned.", Guid.NewGuid());
        await Task.Delay(20);
        var currentNotification = new NotificationItem("user-lead", NotificationType.Comment, "Comment added", "Task comment posted.", Guid.NewGuid());
        olderNotification.MarkAsRead();
        var otherUserNotification = new NotificationItem("user-other", NotificationType.DueSoon, "Reminder", "Other user reminder.", Guid.NewGuid());

        dbContext.Notifications.AddRange(currentNotification, olderNotification, otherUserNotification);
        await dbContext.SaveChangesAsync();

        var service = new NotificationService(dbContext, new TestCurrentUser("user-lead", "Капарис Андрій", WorkspaceRole.Member));

        var notifications = (await service.ListAsync(CancellationToken.None)).ToArray();

        notifications.Should().HaveCount(2);
        notifications.Should().BeInDescendingOrder(item => item.CreatedAtUtc);
        notifications.First().Title.Should().Be("Comment added");
        notifications.Should().OnlyContain(item => item.Title == "Task assigned" || item.Title == "Comment added");
        notifications.Single(item => item.Title == "Task assigned").IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsReadAsync_updates_owned_notification()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var notification = new NotificationItem("user-backend", NotificationType.Assignment, "Board update", "Status changed.", Guid.NewGuid());
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        var service = new NotificationService(dbContext, new TestCurrentUser("user-backend", "Богдан", WorkspaceRole.Member));

        await service.MarkAsReadAsync(notification.Id, CancellationToken.None);

        var updated = await dbContext.Notifications.FindAsync(notification.Id);
        updated.Should().NotBeNull();
        updated!.IsRead.Should().BeTrue();
    }
}
