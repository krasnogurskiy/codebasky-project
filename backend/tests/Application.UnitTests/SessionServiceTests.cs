using Codebasky.Application.Services;
using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using FluentAssertions;

namespace Codebasky.Application.UnitTests;

public sealed class SessionServiceTests
{
    [Fact]
    public async Task GetSessionAsync_returns_workspace_context_for_current_member()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var workspace = new Workspace("Codebasky Semester Team", "Semester delivery workspace");
        var member = new WorkspaceMember(workspace.Id, "user-manager", "Красногурський Андрій", WorkspaceRole.Manager);

        dbContext.Workspaces.Add(workspace);
        dbContext.WorkspaceMembers.Add(member);
        await dbContext.SaveChangesAsync();

        var service = new SessionService(dbContext, new TestCurrentUser("user-manager", "Красногурський Андрій", WorkspaceRole.Manager));

        var session = await service.GetSessionAsync(CancellationToken.None);

        session.UserId.Should().Be("user-manager");
        session.DisplayName.Should().Be("Красногурський Андрій");
        session.Role.Should().Be(WorkspaceRole.Manager);
        session.WorkspaceId.Should().Be(workspace.Id);
        session.WorkspaceName.Should().Be("Codebasky Semester Team");
    }

    [Fact]
    public async Task GetSessionAsync_throws_when_user_has_no_membership()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Workspaces.Add(new Workspace("Codebasky Semester Team", "Semester delivery workspace"));
        await dbContext.SaveChangesAsync();

        var service = new SessionService(dbContext, new TestCurrentUser("missing-user", "Missing User", WorkspaceRole.Guest));

        var action = () => service.GetSessionAsync(CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not attached to any workspace*");
    }
}
