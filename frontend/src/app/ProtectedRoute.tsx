import { Navigate, Outlet } from 'react-router-dom'
import { useSession } from '../auth/SessionContext'
import type { WorkspaceRole } from '../api/types'

type ProtectedRouteProps = {
  children?: React.ReactNode
  requiredRole?: WorkspaceRole
}

export function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
  const { isLoading, localSession } = useSession()

  if (isLoading) {
    return <div className="shell-main">Loading session…</div>
  }

  if (!localSession) {
    return <Navigate to="/login" replace />
  }

  if (requiredRole && localSession.role !== requiredRole) {
    return (
      <div className="shell-main">
        <section className="surface-card stack">
          <h1>Access restricted</h1>
          <p className="muted">This view is only available for the manager role in the current MVP.</p>
        </section>
      </div>
    )
  }

  return children ? <>{children}</> : <Outlet />
}
