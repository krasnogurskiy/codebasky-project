import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import type { ProjectSummaryDto, TaskSummaryDto, WorkItemPriority, WorkItemStatus, WorkspaceMemberDto } from '../api/types'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'

const statusOrder: WorkItemStatus[] = ['Todo', 'InProgress', 'Done']

export function BoardPage() {
  const navigate = useNavigate()
  const { localSession, serverSession } = useSession()
  const { taskRevision } = useRealtime()
  const [projects, setProjects] = useState<ProjectSummaryDto[]>([])
  const [members, setMembers] = useState<WorkspaceMemberDto[]>([])
  const [selectedProjectId, setSelectedProjectId] = useState<string>('')
  const [tasks, setTasks] = useState<TaskSummaryDto[]>([])
  const [search, setSearch] = useState('')
  const [filter, setFilter] = useState<'all' | 'mine' | 'overdue'>('all')
  const [newTask, setNewTask] = useState({
    title: '',
    description: '',
    assigneeUserId: '',
    dueDateUtc: '',
    priority: 'Medium' as WorkItemPriority,
    requirementKey: '',
  })
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!localSession || !serverSession) {
      return
    }

    void Promise.all([api.getWorkspace(localSession), api.listProjects(serverSession.workspaceId, localSession)])
      .then(([workspace, projectData]) => {
        setMembers(workspace.members)
        setProjects(projectData)
        if (!selectedProjectId && projectData[0]) {
          setSelectedProjectId(projectData[0].id)
        }
      })
      .catch((requestError) => setError(requestError instanceof Error ? requestError.message : 'Unable to load board.'))
  }, [localSession, serverSession, selectedProjectId])

  useEffect(() => {
    if (!localSession || !selectedProjectId) {
      return
    }

    void api
      .listTasks(
        {
          projectId: selectedProjectId,
          assignee: filter === 'mine' ? 'mine' : undefined,
        },
        localSession,
      )
      .then((taskData) => {
        setTasks(taskData)
        setError(null)
      })
      .catch((requestError) => setError(requestError instanceof Error ? requestError.message : 'Unable to load tasks.'))
  }, [filter, localSession, selectedProjectId, taskRevision])

  const filteredTasks = useMemo(() => {
    return tasks.filter((task) => {
      if (filter === 'overdue' && !task.isOverdue) {
        return false
      }

      const haystack = `${task.title} ${task.description}`.toLowerCase()
      return haystack.includes(search.trim().toLowerCase())
    })
  }, [filter, search, tasks])

  if (!localSession || !serverSession) {
    return <div>Loading…</div>
  }

  const session = localSession

  async function handleCreateTask(event: FormEvent) {
    event.preventDefault()
    if (!newTask.title.trim()) {
      return
    }

    const assignee = members.find((member) => member.userId === newTask.assigneeUserId)
    await api.createTask(
      {
        projectId: selectedProjectId,
        title: newTask.title,
        description: newTask.description,
        assigneeUserId: assignee?.userId ?? null,
        assigneeDisplayName: assignee?.displayName ?? null,
        dueDateUtc: newTask.dueDateUtc ? new Date(newTask.dueDateUtc).toISOString() : null,
        priority: newTask.priority,
        requirementKey: newTask.requirementKey || null,
      },
      session,
    )

    const taskData = await api.listTasks({ projectId: selectedProjectId }, session)
    setTasks(taskData)
    setNewTask({
      title: '',
      description: '',
      assigneeUserId: '',
      dueDateUtc: '',
      priority: 'Medium',
      requirementKey: '',
    })
  }

  async function updateStatus(task: TaskSummaryDto, status: WorkItemStatus) {
    await api.updateTask(
      task.id,
      {
        title: task.title,
        description: task.description,
        status,
        priority: task.priority,
        assigneeUserId: task.assigneeUserId,
        assigneeDisplayName: task.assigneeDisplayName,
        dueDateUtc: task.dueDateUtc,
        requirementKey: task.requirementKey,
      },
      session,
    )

    const refreshed = await api.listTasks({ projectId: selectedProjectId }, session)
    setTasks(refreshed)
  }

  return (
    <div className="stack">
      <section className="page-header">
        <div>
          <h1>Board</h1>
          <p>Track task status, ownership and due dates in the active project.</p>
        </div>
      </section>

      {error && <div className="error-banner">{error}</div>}

      <section className="surface-card stack">
        <div className="filters-row" style={{ gridTemplateColumns: '1.4fr 0.8fr auto auto auto' }}>
          <label>
            Search tasks
            <input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search tasks by title or description"
            />
          </label>
          <label>
            Project
            <select value={selectedProjectId} onChange={(event) => setSelectedProjectId(event.target.value)}>
              {projects.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </select>
          </label>
          <button className={filter === 'all' ? 'button-primary' : 'button-secondary'} onClick={() => setFilter('all')}>
            All
          </button>
          <button className={filter === 'mine' ? 'button-primary' : 'button-secondary'} onClick={() => setFilter('mine')}>
            My tasks
          </button>
          <button className={filter === 'overdue' ? 'button-primary' : 'button-secondary'} onClick={() => setFilter('overdue')}>
            Overdue
          </button>
        </div>
      </section>

      {session.role !== 'Guest' && (
        <section className="surface-card">
          <h2>New task</h2>
          <form className="task-form" onSubmit={handleCreateTask}>
            <label>
              Title
              <input value={newTask.title} onChange={(event) => setNewTask({ ...newTask, title: event.target.value })} />
            </label>
            <label>
              Requirement
              <input
                value={newTask.requirementKey}
                onChange={(event) => setNewTask({ ...newTask, requirementKey: event.target.value })}
                placeholder="FR-12"
              />
            </label>
            <label className="full-width">
              Description
              <textarea
                value={newTask.description}
                onChange={(event) => setNewTask({ ...newTask, description: event.target.value })}
              />
            </label>
            <label>
              Assignee
              <select
                value={newTask.assigneeUserId}
                onChange={(event) => setNewTask({ ...newTask, assigneeUserId: event.target.value })}
              >
                <option value="">Unassigned</option>
                {members.map((member) => (
                  <option key={member.userId} value={member.userId}>
                    {member.displayName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Due date
              <input
                type="date"
                value={newTask.dueDateUtc}
                onChange={(event) => setNewTask({ ...newTask, dueDateUtc: event.target.value })}
              />
            </label>
            <label>
              Priority
              <select value={newTask.priority} onChange={(event) => setNewTask({ ...newTask, priority: event.target.value as WorkItemPriority })}>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Critical">Critical</option>
              </select>
            </label>
            <div className="button-row">
              <button className="button-primary" type="submit">
                Create task
              </button>
            </div>
          </form>
        </section>
      )}

      <section className="task-board">
        {statusOrder.map((status) => (
          <div key={status} className="column">
            <h3>{status === 'Todo' ? 'To Do' : status === 'InProgress' ? 'In Progress' : 'Done'}</h3>
            {filteredTasks
              .filter((task) => task.status === status)
              .map((task) => (
                <article key={task.id} className="task-card">
                  <button className="linklike" onClick={() => navigate(`/tasks/${task.id}`)}>
                    {task.title}
                  </button>
                  <p className="muted">{task.description}</p>
                  <div className="inline-row">
                    <span className={`tag status-${task.status.toLowerCase()}`}>{task.status}</span>
                    <span className={`tag priority-${task.priority.toLowerCase()}`}>{task.priority}</span>
                  </div>
                  <span className="note">Owner: {task.assigneeDisplayName ?? 'Unassigned'}</span>
                  <footer>
                    <select value={task.status} onChange={(event) => void updateStatus(task, event.target.value as WorkItemStatus)}>
                      <option value="Todo">To Do</option>
                      <option value="InProgress">In Progress</option>
                      <option value="Done">Done</option>
                    </select>
                    {task.isOverdue && <span className="tag">Overdue</span>}
                  </footer>
                </article>
              ))}
            {filteredTasks.every((task) => task.status !== status) && <div className="empty-state">No tasks in this column.</div>}
          </div>
        ))}
      </section>
    </div>
  )
}
