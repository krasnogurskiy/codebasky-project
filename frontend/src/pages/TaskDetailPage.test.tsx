import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { vi } from 'vitest'
import { api } from '../api/client'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'
import { TaskDetailPage } from './TaskDetailPage'

vi.mock('../api/client', () => ({
  api: {
    getTask: vi.fn(),
    getWorkspace: vi.fn(),
    updateTask: vi.fn(),
    addComment: vi.fn(),
  },
}))

vi.mock('../auth/SessionContext', () => ({
  useSession: vi.fn(),
}))

vi.mock('../realtime/RealtimeContext', () => ({
  useRealtime: vi.fn(),
}))

const mockedApi = vi.mocked(api)
const mockedUseSession = vi.mocked(useSession)
const mockedUseRealtime = vi.mocked(useRealtime)

describe('TaskDetailPage', () => {
  beforeEach(() => {
    mockedUseRealtime.mockReturnValue({ taskRevision: 0, notificationRevision: 0 })
    mockedUseSession.mockReturnValue({
      localSession: { userId: 'user-1', displayName: 'Manager', role: 'Manager' },
      serverSession: null,
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })
    mockedApi.getWorkspace.mockResolvedValue({
      workspaceId: 'workspace-1',
      name: 'Codebasky Semester Team',
      description: 'Shared workspace',
      members: [{ userId: 'user-1', displayName: 'Manager', role: 'Manager' }],
      projects: [],
      openTasks: 0,
      dueThisWeek: 0,
    })
    mockedApi.getTask.mockResolvedValue({
      id: 'task-1',
      projectId: 'project-1',
      projectName: 'Codebasky MVP',
      title: 'Implement notification center',
      description: 'Comments and reminders',
      status: 'InProgress',
      priority: 'High',
      assigneeUserId: 'user-1',
      assigneeDisplayName: 'Manager',
      dueDateUtc: '2026-05-08T00:00:00Z',
      requirementKey: 'FR-12',
      activities: [{ id: 'a1', actorDisplayName: 'Manager', message: 'Task created', createdAtUtc: new Date().toISOString() }],
      comments: [{ id: 'c1', authorDisplayName: 'Bogdan', body: 'Realtime payload is ready', createdAtUtc: new Date().toISOString() }],
    })
  })

  it('renders comments and activity log', async () => {
    render(
      <MemoryRouter initialEntries={['/tasks/task-1']}>
        <Routes>
          <Route path="/tasks/:taskId" element={<TaskDetailPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('Implement notification center')).toBeInTheDocument()
    expect(screen.getByText('Realtime payload is ready')).toBeInTheDocument()
    expect(screen.getByText('Task created')).toBeInTheDocument()
  })

  it('prefills editable task metadata', async () => {
    render(
      <MemoryRouter initialEntries={['/tasks/task-1']}>
        <Routes>
          <Route path="/tasks/:taskId" element={<TaskDetailPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByDisplayValue('Implement notification center')).toBeInTheDocument()
    expect(screen.getByDisplayValue('FR-12')).toBeInTheDocument()
    expect(screen.getByDisplayValue('2026-05-08')).toBeInTheDocument()
  })
})
