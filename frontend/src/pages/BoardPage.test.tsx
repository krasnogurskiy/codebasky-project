import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import { api } from '../api/client'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'
import { BoardPage } from './BoardPage'

vi.mock('../api/client', () => ({
  api: {
    getWorkspace: vi.fn(),
    listProjects: vi.fn(),
    listTasks: vi.fn(),
    createTask: vi.fn(),
    updateTask: vi.fn(),
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

describe('BoardPage', () => {
  beforeEach(() => {
    mockedUseRealtime.mockReturnValue({ taskRevision: 0, notificationRevision: 0 })
    mockedApi.getWorkspace.mockResolvedValue({
      workspaceId: 'workspace-1',
      name: 'Codebasky Semester Team',
      description: 'Shared workspace',
      members: [{ userId: 'user-1', displayName: 'Manager', role: 'Manager' }],
      projects: [],
      openTasks: 7,
      dueThisWeek: 3,
    })
    mockedApi.listProjects.mockResolvedValue([
      { id: 'project-1', name: 'Codebasky MVP', summary: 'Core delivery', status: 'Active', openTasks: 5 },
    ])
    mockedApi.listTasks.mockResolvedValue([
      {
        id: 'task-1',
        projectId: 'project-1',
        projectName: 'Codebasky MVP',
        title: 'Implement notification center',
        description: 'Comments and reminders',
        status: 'InProgress',
        priority: 'High',
        assigneeUserId: 'user-1',
        assigneeDisplayName: 'Manager',
        dueDateUtc: null,
        requirementKey: 'FR-12',
        isOverdue: false,
        updatedAtUtc: new Date().toISOString(),
      },
      {
        id: 'task-2',
        projectId: 'project-1',
        projectName: 'Codebasky MVP',
        title: 'Backup policy',
        description: 'Recoverability',
        status: 'Todo',
        priority: 'Critical',
        assigneeUserId: null,
        assigneeDisplayName: null,
        dueDateUtc: null,
        requirementKey: 'NFR-DOC-06',
        isOverdue: true,
        updatedAtUtc: new Date().toISOString(),
      },
    ])
  })

  it('filters visible tasks by search input', async () => {
    mockedUseSession.mockReturnValue({
      localSession: { userId: 'user-1', displayName: 'Manager', role: 'Manager' },
      serverSession: { userId: 'user-1', displayName: 'Manager', role: 'Manager', workspaceId: 'workspace-1', workspaceName: 'Codebasky Semester Team' },
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })

    const user = userEvent.setup()

    render(
      <MemoryRouter>
        <BoardPage />
      </MemoryRouter>,
    )

    expect(await screen.findByText('Implement notification center')).toBeInTheDocument()
    await user.type(screen.getByPlaceholderText(/search tasks/i), 'backup')

    await waitFor(() => {
      expect(screen.queryByText('Implement notification center')).not.toBeInTheDocument()
    })
    expect(screen.getByText('Backup policy')).toBeInTheDocument()
  })

  it('hides task creation for guest users', async () => {
    mockedUseSession.mockReturnValue({
      localSession: { userId: 'user-guest', displayName: 'Guest', role: 'Guest' },
      serverSession: { userId: 'user-guest', displayName: 'Guest', role: 'Guest', workspaceId: 'workspace-1', workspaceName: 'Codebasky Semester Team' },
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })

    render(
      <MemoryRouter>
        <BoardPage />
      </MemoryRouter>,
    )

    expect(await screen.findByText('Implement notification center')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /create task/i })).not.toBeInTheDocument()
  })
})
