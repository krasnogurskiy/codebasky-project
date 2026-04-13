using System.Net;
using System.Net.Http.Json;
using Codebasky.Application.Models;
using FluentAssertions;

namespace Codebasky.Api.FunctionalTests;

public class TaskAndCommentTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public TaskAndCommentTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ListTasks_Should_Return_Seeded_Tasks()
    {
        using var client = _factory.CreateAuthorizedClient("user-manager", "Manager");
        var session = await client.ReadAsAsync<SessionDto>("/api/session");
        var projects = await client.ReadAsAsync<ProjectSummaryDto[]>($"/api/projects?workspaceId={session!.WorkspaceId}");

        var tasks = await client.ReadAsAsync<TaskSummaryDto[]>($"/api/tasks?projectId={projects![0].Id}");

        tasks.Should().NotBeNull();
        tasks!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateTask_Should_Return_Created_Task()
    {
        using var client = _factory.CreateAuthorizedClient("user-lead", "Member");
        var session = await client.ReadAsAsync<SessionDto>("/api/session");
        var projects = await client.ReadAsAsync<ProjectSummaryDto[]>($"/api/projects?workspaceId={session!.WorkspaceId}");

        var response = await client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest(
            projects![0].Id,
            "Write smoke tests",
            "Prepare smoke coverage for the MVP",
            "user-lead",
            "Lead",
            DateTime.UtcNow.AddDays(1),
            Domain.Enums.WorkItemPriority.High,
            "FR-14"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadAsAsync<TaskSummaryDto>();
        created!.Title.Should().Be("Write smoke tests");
    }

    [Fact]
    public async Task UpdateTask_Should_Change_Status()
    {
        using var client = _factory.CreateAuthorizedClient("user-lead", "Member");
        var tasks = await client.ReadAsAsync<TaskSummaryDto[]>("/api/tasks");
        var target = tasks!.First(task => task.Status == Domain.Enums.WorkItemStatus.Todo);

        var response = await client.PutAsJsonAsync($"/api/tasks/{target.Id}", new UpdateTaskRequest(
            target.Title,
            target.Description,
            Domain.Enums.WorkItemStatus.InProgress,
            target.Priority,
            target.AssigneeUserId,
            target.AssigneeDisplayName,
            target.DueDateUtc,
            target.RequirementKey));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadAsAsync<TaskSummaryDto>();
        updated!.Status.ToString().Should().Be("InProgress");
    }

    [Fact]
    public async Task GetTask_Should_Return_Details_And_Comments()
    {
        using var client = _factory.CreateAuthorizedClient("user-manager", "Manager");
        var tasks = await client.ReadAsAsync<TaskSummaryDto[]>("/api/tasks");

        var details = await client.ReadAsAsync<TaskDetailsDto>($"/api/tasks/{tasks![0].Id}");

        details.Should().NotBeNull();
        details!.Activities.Should().NotBeNull();
        details.Comments.Should().NotBeNull();
    }

    [Fact]
    public async Task AddComment_Should_Return_Created_Comment()
    {
        using var client = _factory.CreateAuthorizedClient("user-backend", "Member");
        var tasks = await client.ReadAsAsync<TaskSummaryDto[]>("/api/tasks");

        var response = await client.PostAsJsonAsync($"/api/tasks/{tasks![0].Id}/comments", new AddCommentRequest("Smoke comment"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var comment = await response.Content.ReadAsAsync<TaskCommentDto>();
        comment!.Body.Should().Be("Smoke comment");
    }

    [Fact]
    public async Task Guest_Should_Not_Add_Comment()
    {
        using var client = _factory.CreateAuthorizedClient("user-guest", "Guest");
        var tasks = await client.ReadAsAsync<TaskSummaryDto[]>("/api/tasks");

        var response = await client.PostAsJsonAsync($"/api/tasks/{tasks![0].Id}/comments", new AddCommentRequest("Blocked"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
