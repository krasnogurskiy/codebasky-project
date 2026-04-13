import type {
  AnalyticsDto,
  LocalUserSession,
  NotificationDto,
  ProjectSummaryDto,
  SessionDto,
  TaskCommentDto,
  TaskDetailsDto,
  TaskSummaryDto,
  WorkspaceOverviewDto,
} from './types'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export class ApiError extends Error {
  public readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

function buildHeaders(session?: LocalUserSession, init?: RequestInit) {
  const headers = new Headers(init?.headers)
  headers.set('Accept', 'application/json')

  if (session) {
    const asciiName = toAsciiHeaderValue(session.displayName, session.userId)
    headers.set('X-Debug-UserId', session.userId)
    headers.set('X-Debug-UserName', asciiName)
    headers.set('X-Debug-Role', session.role)
  }

  if (init?.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  return headers
}

function toAsciiHeaderValue(value: string, fallback: string) {
  const normalized = Array.from(value.normalize('NFKD'))
    .filter((character) => character.charCodeAt(0) >= 32 && character.charCodeAt(0) <= 126)
    .join('')
    .trim()
  return normalized || fallback
}

async function requestJson<T>(path: string, session?: LocalUserSession, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: buildHeaders(session, init),
  })

  if (!response.ok) {
    const message = await response.text()
    throw new ApiError(message || `Request failed with ${response.status}`, response.status)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const api = {
  getSession: (session: LocalUserSession) => requestJson<SessionDto>('/api/session', session),
  getWorkspace: (session: LocalUserSession) => requestJson<WorkspaceOverviewDto>('/api/workspaces/current', session),
  listProjects: (workspaceId: string, session: LocalUserSession) =>
    requestJson<ProjectSummaryDto[]>(`/api/projects?workspaceId=${workspaceId}`, session),
  createProject: (body: { workspaceId: string; name: string; summary: string }, session: LocalUserSession) =>
    requestJson<ProjectSummaryDto>('/api/projects', session, { method: 'POST', body: JSON.stringify(body) }),
  listTasks: (params: { projectId?: string; assignee?: string }, session: LocalUserSession) => {
    const search = new URLSearchParams()
    if (params.projectId) search.set('projectId', params.projectId)
    if (params.assignee) search.set('assignee', params.assignee)
    const query = search.toString()
    return requestJson<TaskSummaryDto[]>(`/api/tasks${query ? `?${query}` : ''}`, session)
  },
  getTask: (taskId: string, session: LocalUserSession) => requestJson<TaskDetailsDto>(`/api/tasks/${taskId}`, session),
  createTask: (
    body: {
      projectId: string
      title: string
      description: string
      assigneeUserId?: string | null
      assigneeDisplayName?: string | null
      dueDateUtc?: string | null
      priority: string
      requirementKey?: string | null
    },
    session: LocalUserSession,
  ) => requestJson<TaskSummaryDto>('/api/tasks', session, { method: 'POST', body: JSON.stringify(body) }),
  updateTask: (
    taskId: string,
    body: {
      title: string
      description: string
      status: string
      priority: string
      assigneeUserId?: string | null
      assigneeDisplayName?: string | null
      dueDateUtc?: string | null
      requirementKey?: string | null
    },
    session: LocalUserSession,
  ) => requestJson<TaskSummaryDto>(`/api/tasks/${taskId}`, session, { method: 'PUT', body: JSON.stringify(body) }),
  addComment: (taskId: string, body: { body: string }, session: LocalUserSession) =>
    requestJson<TaskCommentDto>(`/api/tasks/${taskId}/comments`, session, { method: 'POST', body: JSON.stringify(body) }),
  getAnalytics: (session: LocalUserSession) => requestJson<AnalyticsDto>('/api/analytics', session),
  getNotifications: (session: LocalUserSession) => requestJson<NotificationDto[]>('/api/notifications', session),
  markNotificationRead: (id: string, session: LocalUserSession) =>
    requestJson<void>(`/api/notifications/${id}/read`, session, { method: 'POST' }),
}
