using Codebasky.Domain.Common;

namespace Codebasky.Domain.Entities;

public class Workspace : AuditableEntity
{
    private readonly List<WorkspaceMember> _members = [];
    private readonly List<Project> _projects = [];

    private Workspace()
    {
    }

    public Workspace(string name, string description)
    {
        Rename(name, description);
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<WorkspaceMember> Members => _members;

    public IReadOnlyCollection<Project> Projects => _projects;

    public void Rename(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workspace name is required.", nameof(name));
        }

        Name = name.Trim();
        Description = description.Trim();
        Touch();
    }

    public WorkspaceMember AddMember(string userId, string displayName, Enums.WorkspaceRole role)
    {
        if (_members.Any(member => member.UserId == userId))
        {
            throw new InvalidOperationException("The user is already a member of the workspace.");
        }

        var member = new WorkspaceMember(Id, userId, displayName, role);
        _members.Add(member);
        Touch();
        return member;
    }

    public Project AddProject(string name, string summary)
    {
        var project = new Project(Id, name, summary);
        _projects.Add(project);
        Touch();
        return project;
    }
}
