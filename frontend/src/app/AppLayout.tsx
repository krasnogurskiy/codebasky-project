import { useEffect, useState } from 'react'
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import type { NotificationDto } from '../api/types'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'

export function AppLayout() {
  const location = useLocation()
  const navigate = useNavigate()
  const { localSession, serverSession, logout, error } = useSession()
  const { notificationRevision } = useRealtime()
  const [notifications, setNotifications] = useState<NotificationDto[]>([])
  const [showNotifications, setShowNotifications] = useState(false)

  useEffect(() => {
    if (!localSession) {
      return
    }

    void api.getNotifications(localSession).then(setNotifications).catch(() => setNotifications([]))
  }, [localSession, notificationRevision])

  if (!localSession) {
    return null
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand-block">
          <div>
            <div className="brand-title">Codebasky</div>
            <div className="brand-subtitle">{serverSession?.workspaceName ?? 'Workspace loading…'}</div>
          </div>
          <nav>
            <NavLink to="/workspace" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
              Workspace
            </NavLink>
            <NavLink to="/board" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
              Board
            </NavLink>
            <NavLink to="/analytics" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
              Analytics
            </NavLink>
          </nav>
        </div>

        <div className="topbar-actions">
          <span className="tag">{localSession.role}</span>
          <button className="button-secondary" onClick={() => setShowNotifications((value) => !value)}>
            Notifications ({notifications.filter((item) => !item.isRead).length})
          </button>
          <button
            className="button-secondary"
            onClick={() => {
              logout()
              navigate('/login')
            }}
          >
            Logout
          </button>
        </div>
      </header>

      {showNotifications && (
        <aside className="notifications-panel">
          {notifications.length === 0 ? (
            <section className="surface-card">No notifications yet.</section>
          ) : (
            notifications.map((notification) => (
              <article key={notification.id} className="notification-card">
                <div className="inline-row">
                  <span className="tag">{notification.type}</span>
                  {!notification.isRead && <span className="tag">Unread</span>}
                </div>
                <strong>{notification.title}</strong>
                <span className="muted">{notification.message}</span>
                <div className="button-row">
                  {notification.taskItemId && (
                    <button className="button-secondary" onClick={() => navigate(`/tasks/${notification.taskItemId}`)}>
                      Open task
                    </button>
                  )}
                  {!notification.isRead && (
                    <button
                      className="button-primary"
                      onClick={async () => {
                        await api.markNotificationRead(notification.id, localSession)
                        const updated = await api.getNotifications(localSession)
                        setNotifications(updated)
                      }}
                    >
                      Mark as read
                    </button>
                  )}
                </div>
              </article>
            ))
          )}
        </aside>
      )}

      <main className="shell-main">
        {error && location.pathname !== '/login' && <div className="error-banner">{error}</div>}
        <Outlet />
      </main>
    </div>
  )
}
