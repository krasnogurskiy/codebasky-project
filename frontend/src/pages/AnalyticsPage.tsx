import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import type { AnalyticsDto } from '../api/types'
import { useSession } from '../auth/SessionContext'
import { useRealtime } from '../realtime/RealtimeContext'

export function AnalyticsPage() {
  const navigate = useNavigate()
  const { localSession } = useSession()
  const { taskRevision } = useRealtime()
  const [analytics, setAnalytics] = useState<AnalyticsDto | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!localSession) {
      return
    }

    void api
      .getAnalytics(localSession)
      .then((data) => {
        setAnalytics(data)
        setError(null)
      })
      .catch((requestError) => setError(requestError instanceof Error ? requestError.message : 'Unable to load analytics.'))
  }, [localSession, taskRevision])

  return (
    <div className="stack">
      <section className="page-header">
        <div>
          <h1>Sprint health snapshot</h1>
          <p>Manager view of throughput, overdue items and current delivery risks.</p>
        </div>
      </section>

      {error && <div className="error-banner">{error}</div>}

      <section className="summary-grid">
        <article className="surface-card metric-card">
          <span className="metric-label">Done this sprint</span>
          <span className="metric-value">{analytics?.doneThisSprint ?? 0}</span>
        </article>
        <article className="surface-card metric-card">
          <span className="metric-label">In progress</span>
          <span className="metric-value">{analytics?.inProgress ?? 0}</span>
        </article>
        <article className="surface-card metric-card">
          <span className="metric-label">Overdue</span>
          <span className="metric-value">{analytics?.overdue ?? 0}</span>
        </article>
      </section>

      <section className="analytics-grid">
        <article className="surface-card">
          <h2>Throughput</h2>
          <div className="chart">
            {analytics?.throughput.map((bar) => (
              <div key={bar.label} className="chart-bar">
                <div style={{ height: `${Math.max(18, bar.value * 22)}px` }} />
                <strong>{bar.value}</strong>
                <span className="note">{bar.label}</span>
              </div>
            ))}
          </div>
        </article>

        <div className="stack">
          <article className="surface-card">
            <h2>Risks</h2>
            <div className="stack">
              {analytics?.risks.map((risk) => (
                <article key={risk.title} className="risk-card">
                  <strong>{risk.title}</strong>
                  <span className="muted">{risk.detail}</span>
                </article>
              ))}
            </div>
          </article>

          <article className="surface-card">
            <h2>Overdue focus</h2>
            {analytics?.overdueFocus ? (
              <div className="stack">
                <strong>{analytics.overdueFocus.title}</strong>
                <span className="note">Owner: {analytics.overdueFocus.owner ?? 'Unassigned'}</span>
                <span className="note">Requirement: {analytics.overdueFocus.requirementKey ?? 'Not linked'}</span>
                <button className="button-primary" onClick={() => navigate(`/tasks/${analytics.overdueFocus?.taskId}`)}>
                  Open task
                </button>
              </div>
            ) : (
              <div className="empty-state">No overdue items right now.</div>
            )}
          </article>
        </div>
      </section>
    </div>
  )
}
