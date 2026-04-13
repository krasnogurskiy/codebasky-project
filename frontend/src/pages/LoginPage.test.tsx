import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import { useSession } from '../auth/SessionContext'
import { LoginPage } from './LoginPage'

const navigate = vi.fn()

vi.mock('../auth/SessionContext', () => ({
  useSession: vi.fn(),
}))

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return {
    ...actual,
    useNavigate: () => navigate,
  }
})

const mockedUseSession = vi.mocked(useSession)

describe('LoginPage', () => {
  beforeEach(() => {
    navigate.mockReset()
    mockedUseSession.mockReturnValue({
      localSession: null,
      serverSession: null,
      isLoading: false,
      error: null,
      login: vi.fn().mockResolvedValue(undefined),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })
  })

  it('renders all seeded login options', () => {
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>,
    )

    expect(screen.getByText('Красногурський Андрій')).toBeInTheDocument()
    expect(screen.getByText('Капарис Андрій')).toBeInTheDocument()
    expect(screen.getByText('Богдан')).toBeInTheDocument()
    expect(screen.getByText('Stakeholder Viewer')).toBeInTheDocument()
  })

  it('logs in with the selected user and navigates to workspace', async () => {
    const user = userEvent.setup()
    const login = vi.fn().mockResolvedValue(undefined)
    mockedUseSession.mockReturnValue({
      localSession: null,
      serverSession: null,
      isLoading: false,
      error: null,
      login,
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>,
    )

    await user.click(screen.getByRole('button', { name: /богдан/i }))
    await user.click(screen.getByRole('button', { name: /open workspace/i }))

    expect(login).toHaveBeenCalledWith({
      userId: 'user-backend',
      displayName: 'Богдан',
      role: 'Member',
    })
    expect(navigate).toHaveBeenCalledWith('/workspace')
  })
})
