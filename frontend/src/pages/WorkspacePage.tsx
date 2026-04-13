import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { api } from '../api/client'
import type { ProjectSummaryDto, WorkspaceOverviewDto } from '../api/types'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'

export function WorkspacePage() {
  const { localSession, serverSession } = useSession()
  const { taskRevision } = useRealtime()
  const [workspace, setWorkspace] = useState<WorkspaceOverviewDto | null>(null)
  const [projects, setProjects] = useState<ProjectSummaryDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [projectName, setProjectName] = useState('')
  const [projectSummary, setProjectSummary] = useState('')

  useEffect(() => {
    if (!localSession || !serverSession) {
      return
    }

    void Promise.all([api.getWorkspace(localSession), api.listProjects(serverSession.workspaceId, localSession)])
      .then(([workspaceData, projectData]) => {
        setWorkspace(workspaceData)
        setProjects(projectData)
        setError(null)
      })
      .catch((requestError) => {
        setError(requestError instanceof Error ? requestError.message : 'Unable to load workspace.')
      })
  }, [localSession, serverSession, taskRevision])

  const nextActions = useMemo(
    () => ['Lock stale task updates', 'Finish reminders', 'Validate manager analytics'],
    [],
  )

  if (!localSession || !serverSession) {
    return <div>Loading…</div>
  }

  const session = localSession
  const workspaceSession = serverSession

  async function handleCreateProject(event: FormEvent) {
    event.preventDefault()

    if (!projectName.trim()) {
      return
    }

    const created = await api.createProject(
      {
        workspaceId: workspaceSession.workspaceId,
        name: projectName,
        summary: projectSummary,
      },
      session,
    )

    setProjects((current) => [...current, created])
    setProjectName('')
    setProjectSummary('')
  }

  return (
    <div className="stack">
      <section className="page-header">
        <div>
          <h1>{workspace?.name ?? 'Workspace overview'}</h1>
          <p>{workspace?.description ?? 'Loading workspace description…'}</p>
        </div>
        <span className="tag">Role: {session.role}</span>
      </section>

      {error && <div className="error-banner">{error}</div>}

      <section className="summary-grid">
        <article className="surface-card metric-card">
          <span className="metric-label">Active projects</span>
          <span className="metric-value">{projects.length}</span>
        </article>
        <article className="surface-card metric-card">
          <span className="metric-label">Open tasks</span>
          <span className="metric-value">{workspace?.openTasks ?? 0}</span>
        </article>
        <article className="surface-card metric-card">
          <span className="metric-label">Due this week</span>
          <span className="metric-value">{workspace?.dueThisWeek ?? 0}</span>
        </article>
      </section>

      <section className="content-grid">
        <div className="stack">
          <article className="surface-card stack">
            <div className="page-header">
              <div>
                <h2>Projects</h2>
                <p>Current delivery scope in the shared workspace.</p>
              </div>
            </div>
            {projects.map((project) => (
              <article key={project.id} className="project-card">
                <div>
                  <strong>{project.name}</strong>
                  <p className="muted">{project.summary}</p>
                </div>
                <footer>
                  <span className="tag">{project.status}</span>
                  <span className="tag">{project.openTasks} open tasks</span>
                </footer>
              </article>
            ))}
            {projects.length === 0 && <div className="empty-state">No projects yet.</div>}
          </article>

          {session.role === 'Manager' && (
            <article className="surface-card">
              <h2>Create project</h2>
              <form className="inline-form" onSubmit={handleCreateProject}>
                <label>
                  Project name
                  <input value={projectName} onChange={(event) => setProjectName(event.target.value)} />
                </label>
                <label>
                  Summary
                  <input value={projectSummary} onChange={(event) => setProjectSummary(event.target.value)} />
                </label>
                <button className="button-primary" type="submit">
                  Add project
                </button>
              </form>
            </article>
          )}
        </div>

        <div className="stack">
          <article className="surface-card">
            <h2>Team</h2>
            <div className="stack">
              {workspace?.members.map((member) => (
                <article key={member.userId} className="team-card">
                  <strong>{member.displayName}</strong>
                  <span className="tag">{member.role}</span>
                </article>
              ))}
            </div>
          </article>

          <article className="surface-card">
            <h2>Next actions</h2>
            <div className="stack">
              {nextActions.map((item) => (
                <span key={item} className="tag">
                  {item}
                </span>
              ))}
            </div>
          </article>
        </div>
      </section>
    </div>
  )
}
