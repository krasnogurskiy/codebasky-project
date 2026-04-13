using System.Net;
using System.Net.Http.Json;
using Codebasky.Application.Models;
using FluentAssertions;

namespace Codebasky.Api.FunctionalTests;

public class SessionAndAuthTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public SessionAndAuthTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSession_Should_Return_Authenticated_User()
    {
        using var client = _factory.CreateAuthorizedClient("user-manager", "Manager");

        var session = await client.ReadAsAsync<SessionDto>("/api/session");

        session.Should().NotBeNull();
        session!.UserId.Should().Be("user-manager");
        session.Role.ToString().Should().Be("Manager");
    }

    [Fact]
    public async Task GetSession_Should_Return_Unauthorized_Without_Debug_Headers()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/session");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Analytics_Should_Be_Forbidden_For_Member()
    {
        using var client = _factory.CreateAuthorizedClient("user-lead", "Member");

        var response = await client.GetAsync("/api/analytics");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Workspace_Should_Be_Accessible_For_Guest()
    {
        using var client = _factory.CreateAuthorizedClient("user-guest", "Guest");

        var response = await client.GetAsync("/api/workspaces/current");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
