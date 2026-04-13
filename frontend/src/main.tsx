import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App'
import { SessionProvider } from './auth/SessionContext'
import './index.css'
import { RealtimeProvider } from './realtime/RealtimeContext'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <SessionProvider>
      <RealtimeProvider>
        <App />
      </RealtimeProvider>
    </SessionProvider>
  </StrictMode>,
)
