using System.Net;
using System.Net.Http.Json;
using Codebasky.Application.Models;
using FluentAssertions;

namespace Codebasky.Api.FunctionalTests;

public class WorkspaceAndProjectTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public WorkspaceAndProjectTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWorkspace_Should_Return_Members_And_Projects()
    {
        using var client = _factory.CreateAuthorizedClient("user-manager", "Manager");

        var workspace = await client.ReadAsAsync<WorkspaceOverviewDto>("/api/workspaces/current");

        workspace.Should().NotBeNull();
        workspace!.Members.Should().HaveCountGreaterOrEqualTo(4);
        workspace.Projects.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListProjects_Should_Return_Seeded_Project()
    {
        using var client = _factory.CreateAuthorizedClient("user-manager", "Manager");
        var session = await client.ReadAsAsync<SessionDto>("/api/session");

        var projects = await client.ReadAsAsync<ProjectSummaryDto[]>($"/api/projects?workspaceId={session!.WorkspaceId}");

        projects.Should().NotBeNull();
        projects!.Should().Contain(project => project.Name == "Codebasky MVP");
    }

    [Fact]
    public async Task Manager_Should_Create_Project()
    {
        using var client = _factory.CreateAuthorizedClient("user-manager", "Manager");
        var session = await client.ReadAsAsync<SessionDto>("/api/session");

        var response = await client.PostAsJsonAsync("/api/projects", new CreateProjectRequest(session!.WorkspaceId, "New delivery stream", "Testing project creation"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadAsAsync<ProjectSummaryDto>();
        created!.Name.Should().Be("New delivery stream");
    }

    [Fact]
    public async Task Member_Should_Not_Create_Project()
    {
        using var client = _factory.CreateAuthorizedClient("user-lead", "Member");
        var session = await client.ReadAsAsync<SessionDto>("/api/session");

        var response = await client.PostAsJsonAsync("/api/projects", new CreateProjectRequest(session!.WorkspaceId, "Blocked", "Members should not create projects"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
