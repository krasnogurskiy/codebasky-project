import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import { api } from '../api/client'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'
import { AnalyticsPage } from './AnalyticsPage'

vi.mock('../api/client', () => ({
  api: {
    getAnalytics: vi.fn(),
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

describe('AnalyticsPage', () => {
  it('renders metrics and overdue focus', async () => {
    mockedUseSession.mockReturnValue({
      localSession: { userId: 'user-manager', displayName: 'Manager', role: 'Manager' },
      serverSession: null,
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })
    mockedUseRealtime.mockReturnValue({ taskRevision: 0, notificationRevision: 0 })
    mockedApi.getAnalytics.mockResolvedValue({
      totalTasks: 12,
      doneThisSprint: 8,
      inProgress: 3,
      overdue: 1,
      throughput: [
        { label: 'W1', value: 1 },
        { label: 'W2', value: 2 },
        { label: 'W3', value: 3 },
        { label: 'W4', value: 2 },
      ],
      risks: [{ title: 'Overdue tasks require attention', detail: 'One item is overdue.' }],
      overdueFocus: { taskId: 'task-2', title: 'Backup policy', owner: 'Bogdan', requirementKey: 'NFR-DOC-06' },
    })

    render(
      <MemoryRouter>
        <AnalyticsPage />
      </MemoryRouter>,
    )

    expect(await screen.findByText('Sprint health snapshot')).toBeInTheDocument()
    expect(screen.getByText('8')).toBeInTheDocument()
    expect(screen.getByText('Backup policy')).toBeInTheDocument()
  })
})
