import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api } from '../api/client'
import type { TaskDetailsDto, WorkItemPriority, WorkItemStatus, WorkspaceMemberDto } from '../api/types'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'

export function TaskDetailPage() {
  const { taskId } = useParams<{ taskId: string }>()
  const { localSession } = useSession()
  const { taskRevision } = useRealtime()
  const [task, setTask] = useState<TaskDetailsDto | null>(null)
  const [members, setMembers] = useState<WorkspaceMemberDto[]>([])
  const [comment, setComment] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState({
    title: '',
    description: '',
    status: 'Todo' as WorkItemStatus,
    priority: 'Medium' as WorkItemPriority,
    assigneeUserId: '',
    dueDateUtc: '',
    requirementKey: '',
  })

  useEffect(() => {
    if (!localSession || !taskId) {
      return
    }

    void Promise.all([api.getTask(taskId, localSession), api.getWorkspace(localSession)])
      .then(([taskData, workspace]) => {
        setTask(taskData)
        setMembers(workspace.members)
        setForm({
          title: taskData.title,
          description: taskData.description,
          status: taskData.status,
          priority: taskData.priority,
          assigneeUserId: taskData.assigneeUserId ?? '',
          dueDateUtc: taskData.dueDateUtc ? taskData.dueDateUtc.slice(0, 10) : '',
          requirementKey: taskData.requirementKey ?? '',
        })
        setError(null)
      })
      .catch((requestError) => {
        setError(requestError instanceof Error ? requestError.message : 'Unable to load task.')
      })
  }, [localSession, taskId, taskRevision])

  if (!localSession || !taskId) {
    return <div>Loading…</div>
  }

  if (!task) {
    return <div className="surface-card">Loading task details…</div>
  }

  const session = localSession
  const currentTask = task

  async function handleSave(event: FormEvent) {
    event.preventDefault()
    const assignee = members.find((member) => member.userId === form.assigneeUserId)
    const updated = await api.updateTask(
      currentTask.id,
      {
        title: form.title,
        description: form.description,
        status: form.status,
        priority: form.priority,
        assigneeUserId: assignee?.userId ?? null,
        assigneeDisplayName: assignee?.displayName ?? null,
        dueDateUtc: form.dueDateUtc ? new Date(form.dueDateUtc).toISOString() : null,
        requirementKey: form.requirementKey || null,
      },
      session,
    )
    const refreshed = await api.getTask(updated.id, session)
    setTask(refreshed)
  }

  async function handleComment(event: FormEvent) {
    event.preventDefault()
    if (!comment.trim()) {
      return
    }

    await api.addComment(currentTask.id, { body: comment }, session)
    setComment('')
    const refreshed = await api.getTask(currentTask.id, session)
    setTask(refreshed)
  }

  return (
    <div className="stack">
      <section className="page-header">
        <div>
          <Link to="/board" className="note">
            Back to board
          </Link>
          <h1>{currentTask.title}</h1>
          <p>{currentTask.projectName}</p>
        </div>
        <div className="inline-row">
          <span className={`tag status-${currentTask.status.toLowerCase()}`}>{currentTask.status}</span>
          <span className={`tag priority-${currentTask.priority.toLowerCase()}`}>{currentTask.priority}</span>
        </div>
      </section>

      {error && <div className="error-banner">{error}</div>}

      <section className="detail-grid">
        <div className="stack">
          <article className="surface-card">
            <h2>Edit task</h2>
            <form className="task-form" onSubmit={handleSave}>
              <label>
                Title
                <input value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
              </label>
              <label>
                Requirement
                <input value={form.requirementKey} onChange={(event) => setForm({ ...form, requirementKey: event.target.value })} />
              </label>
              <label className="full-width">
                Description
                <textarea value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
              </label>
              <label>
                Status
                <select value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value as WorkItemStatus })}>
                  <option value="Todo">To Do</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Done">Done</option>
                </select>
              </label>
              <label>
                Priority
                <select value={form.priority} onChange={(event) => setForm({ ...form, priority: event.target.value as WorkItemPriority })}>
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </select>
              </label>
              <label>
                Assignee
                <select value={form.assigneeUserId} onChange={(event) => setForm({ ...form, assigneeUserId: event.target.value })}>
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
                <input type="date" value={form.dueDateUtc} onChange={(event) => setForm({ ...form, dueDateUtc: event.target.value })} />
              </label>
              <div className="button-row full-width">
                <button className="button-primary" type="submit">
                  Save changes
                </button>
              </div>
            </form>
          </article>

          <article className="surface-card">
            <h2>Comments</h2>
            <div className="stack">
              {currentTask.comments.map((item) => (
                <article key={item.id} className="comment-card">
                  <strong>{item.authorDisplayName}</strong>
                  <span>{item.body}</span>
                </article>
              ))}
              {currentTask.comments.length === 0 && <div className="empty-state">No comments yet.</div>}
            </div>
            {session.role !== 'Guest' && (
              <form className="comment-form" onSubmit={handleComment} style={{ marginTop: '1rem' }}>
                <label className="full-width">
                  Add comment
                  <textarea value={comment} onChange={(event) => setComment(event.target.value)} />
                </label>
                <div className="button-row">
                  <button className="button-primary" type="submit">
                    Post comment
                  </button>
                </div>
              </form>
            )}
          </article>
        </div>

        <div className="stack">
          <article className="surface-card">
            <h2>Meta</h2>
            <div className="stack">
              <span className="tag">Owner: {currentTask.assigneeDisplayName ?? 'Unassigned'}</span>
              <span className="tag">Due: {currentTask.dueDateUtc ? currentTask.dueDateUtc.slice(0, 10) : 'Not set'}</span>
              <span className="tag">Requirement: {currentTask.requirementKey ?? 'Not linked'}</span>
            </div>
          </article>

          <article className="surface-card">
            <h2>Change log</h2>
            <div className="stack">
              {currentTask.activities.map((item) => (
                <article key={item.id} className="comment-card">
                  <strong>{item.actorDisplayName}</strong>
                  <span>{item.message}</span>
                </article>
              ))}
            </div>
          </article>
        </div>
      </section>
    </div>
  )
}
