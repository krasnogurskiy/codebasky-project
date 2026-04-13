import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { LocalUserSession } from '../api/types'
import { demoUsers } from '../auth/mockUsers'
import { useSession } from '../auth/SessionContext'

export function LoginPage() {
  const navigate = useNavigate()
  const { login, error } = useSession()
  const [selected, setSelected] = useState<LocalUserSession>(demoUsers[0])
  const [submitting, setSubmitting] = useState(false)

  return (
    <div className="login-grid">
      <section className="login-hero">
        <div className="login-copy">
          <span className="tag">Core MVP</span>
          <h1>Team work without context loss.</h1>
          <p>
            Codebasky keeps workspace state, projects, tasks, comments, notifications and manager metrics in one flow.
            This local login mirrors the seeded workspace roles from the backend so we can execute the full Phase 6 test scope.
          </p>
          <div className="surface-card">
            <strong>Included in this build</strong>
            <p className="muted">
              Workspace overview, project creation, board management, task editing, comments, analytics and realtime refresh.
            </p>
          </div>
        </div>
      </section>

      <section className="login-panel">
        <div className="surface-card stack" style={{ width: 'min(100%, 520px)' }}>
          <div>
            <h2>Open workspace</h2>
            <p className="note">Pick the role you want to use for smoke testing and manual scenarios.</p>
          </div>

          <div className="login-users">
            {demoUsers.map((user) => (
              <button
                key={user.userId}
                type="button"
                className={selected.userId === user.userId ? 'user-option active' : 'user-option'}
                onClick={() => setSelected(user)}
              >
                <div style={{ textAlign: 'left' }}>
                  <strong>{user.displayName}</strong>
                  <div className="note">{user.role}</div>
                </div>
                <span className="tag">{user.role}</span>
              </button>
            ))}
          </div>

          {error && <div className="error-banner">{error}</div>}

          <div className="button-row">
            <button
              className="button-primary"
              disabled={submitting}
              onClick={async () => {
                setSubmitting(true)
                try {
                  await login(selected)
                  navigate('/workspace')
                } finally {
                  setSubmitting(false)
                }
              }}
            >
              {submitting ? 'Opening…' : 'Open workspace'}
            </button>
            <button className="button-secondary" onClick={() => navigate('/analytics')}>
              See manager view
            </button>
          </div>
        </div>
      </section>
    </div>
  )
}
