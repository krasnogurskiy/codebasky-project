using Codebasky.Domain.Entities;
using Codebasky.Domain.Enums;
using FluentAssertions;

namespace Codebasky.Domain.UnitTests;

public class WorkspaceTests
{
    [Fact]
    public void Constructor_Should_Set_Name_And_Description()
    {
        var workspace = new Workspace("  Codebasky  ", "  Semester workspace  ");

        workspace.Name.Should().Be("Codebasky");
        workspace.Description.Should().Be("Semester workspace");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Name_Missing()
    {
        var action = () => new Workspace(" ", "desc");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rename_Should_Update_Values()
    {
        var workspace = new Workspace("Codebasky", "desc");

        workspace.Rename("  New workspace  ", "  updated  ");

        workspace.Name.Should().Be("New workspace");
        workspace.Description.Should().Be("updated");
    }

    [Fact]
    public void AddMember_Should_Create_Workspace_Member()
    {
        var workspace = new Workspace("Codebasky", "desc");

        var member = workspace.AddMember("user-1", "Andrii", WorkspaceRole.Manager);

        workspace.Members.Should().ContainSingle();
        member.UserId.Should().Be("user-1");
        member.DisplayName.Should().Be("Andrii");
        member.Role.Should().Be(WorkspaceRole.Manager);
    }

    [Fact]
    public void AddMember_Should_Throw_For_Duplicate_User()
    {
        var workspace = new Workspace("Codebasky", "desc");
        workspace.AddMember("user-1", "Andrii", WorkspaceRole.Manager);

        var action = () => workspace.AddMember("user-1", "Bogdan", WorkspaceRole.Member);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddProject_Should_Create_Active_Project()
    {
        var workspace = new Workspace("Codebasky", "desc");

        var project = workspace.AddProject("MVP", "Summary");

        workspace.Projects.Should().ContainSingle();
        project.Status.Should().Be(ProjectStatus.Active);
        project.WorkspaceId.Should().Be(workspace.Id);
    }
}
