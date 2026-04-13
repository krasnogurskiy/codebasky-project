import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import { api } from '../api/client'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'
import { WorkspacePage } from './WorkspacePage'

vi.mock('../api/client', () => ({
  api: {
    getWorkspace: vi.fn(),
    listProjects: vi.fn(),
    createProject: vi.fn(),
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

describe('WorkspacePage', () => {
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
  })

  it('shows projects and manager creation controls', async () => {
    mockedUseSession.mockReturnValue({
      localSession: { userId: 'user-1', displayName: 'Manager', role: 'Manager' },
      serverSession: { userId: 'user-1', displayName: 'Manager', role: 'Manager', workspaceId: 'workspace-1', workspaceName: 'Codebasky Semester Team' },
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })

    render(
      <MemoryRouter>
        <WorkspacePage />
      </MemoryRouter>,
    )

    expect(await screen.findByText('Codebasky MVP')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /add project/i })).toBeInTheDocument()
  })

  it('hides project creation controls for members', async () => {
    mockedUseSession.mockReturnValue({
      localSession: { userId: 'user-2', displayName: 'Member', role: 'Member' },
      serverSession: { userId: 'user-2', displayName: 'Member', role: 'Member', workspaceId: 'workspace-1', workspaceName: 'Codebasky Semester Team' },
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })

    render(
      <MemoryRouter>
        <WorkspacePage />
      </MemoryRouter>,
    )

    expect(await screen.findByText('Codebasky MVP')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /add project/i })).not.toBeInTheDocument()
  })
})
