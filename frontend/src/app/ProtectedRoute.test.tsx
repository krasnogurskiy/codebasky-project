import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { vi } from 'vitest'
import { ProtectedRoute } from './ProtectedRoute'
import { useSession } from '../auth/SessionContext'

vi.mock('../auth/SessionContext', () => ({
  useSession: vi.fn(),
}))

const mockedUseSession = vi.mocked(useSession)

describe('ProtectedRoute', () => {
  it('redirects unauthenticated users to login', () => {
    mockedUseSession.mockReturnValue({
      localSession: null,
      serverSession: null,
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })

    render(
      <MemoryRouter initialEntries={['/workspace']}>
        <Routes>
          <Route path="/login" element={<div>Login page</div>} />
          <Route path="/workspace" element={<ProtectedRoute><div>Workspace</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(screen.getByText('Login page')).toBeInTheDocument()
  })

  it('renders children when the user is authenticated', () => {
    mockedUseSession.mockReturnValue({
      localSession: { userId: 'user-1', displayName: 'Andrii', role: 'Member' },
      serverSession: null,
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })

    render(
      <MemoryRouter>
        <ProtectedRoute>
          <div>Protected content</div>
        </ProtectedRoute>
      </MemoryRouter>,
    )

    expect(screen.getByText('Protected content')).toBeInTheDocument()
  })

  it('blocks non-manager users from manager-only routes', () => {
    mockedUseSession.mockReturnValue({
      localSession: { userId: 'user-1', displayName: 'Andrii', role: 'Member' },
      serverSession: null,
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshSession: vi.fn(),
    })

    render(
      <MemoryRouter>
        <ProtectedRoute requiredRole="Manager">
          <div>Manager content</div>
        </ProtectedRoute>
      </MemoryRouter>,
    )

    expect(screen.getByText('Access restricted')).toBeInTheDocument()
    expect(screen.queryByText('Manager content')).not.toBeInTheDocument()
  })
})
