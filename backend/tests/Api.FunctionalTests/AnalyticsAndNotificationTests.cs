using System.Net;
using System.Net.Http.Json;
using Codebasky.Application.Models;
using FluentAssertions;

namespace Codebasky.Api.FunctionalTests;

public class AnalyticsAndNotificationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public AnalyticsAndNotificationTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Analytics_Should_Return_Manager_Metrics()
    {
        using var client = _factory.CreateAuthorizedClient("user-manager", "Manager");

        var analytics = await client.ReadAsAsync<AnalyticsDto>("/api/analytics");

        analytics.Should().NotBeNull();
        analytics!.TotalTasks.Should().BeGreaterThan(0);
        analytics.Throughput.Should().HaveCount(4);
    }

    [Fact]
    public async Task Notifications_Should_Return_User_Items()
    {
        using var client = _factory.CreateAuthorizedClient("user-backend", "Member");

        var notifications = await client.ReadAsAsync<NotificationDto[]>("/api/notifications");

        notifications.Should().NotBeNull();
        notifications!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MarkNotificationRead_Should_Return_NoContent()
    {
        using var client = _factory.CreateAuthorizedClient("user-backend", "Member");
        var notifications = await client.ReadAsAsync<NotificationDto[]>("/api/notifications");

        var response = await client.PostAsync($"/api/notifications/{notifications![0].Id}/read", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MarkNotificationRead_Should_Update_Item_State()
    {
        using var client = _factory.CreateAuthorizedClient("user-backend", "Member");
        var notifications = await client.ReadAsAsync<NotificationDto[]>("/api/notifications");
        await client.PostAsync($"/api/notifications/{notifications![0].Id}/read", content: null);

        var refreshed = await client.ReadAsAsync<NotificationDto[]>("/api/notifications");

        refreshed![0].IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task UnknownNotification_Should_Return_InternalServerError()
    {
        using var client = _factory.CreateAuthorizedClient("user-backend", "Member");

        var response = await client.PostAsync($"/api/notifications/{Guid.NewGuid()}/read", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
