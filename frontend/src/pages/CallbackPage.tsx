import { Link } from 'react-router-dom'

export function CallbackPage() {
  return (
    <section className="surface-card stack">
      <h1>Callback route reserved</h1>
      <p className="muted">
        The current MVP runs in local debug-auth mode. This route is reserved for Auth0 callback wiring when external identity is enabled.
      </p>
      <div>
        <Link className="button-primary" to="/login">
          Return to login
        </Link>
      </div>
    </section>
  )
}
