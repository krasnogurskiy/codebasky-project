import { createContext, useContext, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { LocalUserSession, SessionDto } from '../api/types'

type SessionContextValue = {
  localSession: LocalUserSession | null
  serverSession: SessionDto | null
  isLoading: boolean
  error: string | null
  login: (session: LocalUserSession) => Promise<void>
  logout: () => void
  refreshSession: () => Promise<void>
}

const SessionContext = createContext<SessionContextValue | undefined>(undefined)
const storageKey = 'codebasky-session'

export function SessionProvider({ children }: { children: React.ReactNode }) {
  const [localSession, setLocalSession] = useState<LocalUserSession | null>(null)
  const [serverSession, setServerSession] = useState<SessionDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const raw = window.localStorage.getItem(storageKey)
    if (!raw) {
      setIsLoading(false)
      return
    }

    try {
      setLocalSession(JSON.parse(raw) as LocalUserSession)
    } catch {
      window.localStorage.removeItem(storageKey)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    if (!localSession) {
      setServerSession(null)
      return
    }

    void refresh(localSession)
  }, [localSession])

  async function refresh(session: LocalUserSession) {
    try {
      setError(null)
      const data = await api.getSession(session)
      setServerSession(data)
    } catch (requestError) {
      setServerSession(null)
      setError(requestError instanceof Error ? requestError.message : 'Unable to load session.')
    }
  }

  async function login(session: LocalUserSession) {
    window.localStorage.setItem(storageKey, JSON.stringify(session))
    setLocalSession(session)
    await refresh(session)
  }

  function logout() {
    window.localStorage.removeItem(storageKey)
    setLocalSession(null)
    setServerSession(null)
    setError(null)
  }

  const value: SessionContextValue = {
    localSession,
    serverSession,
    isLoading,
    error,
    login,
    logout,
    refreshSession: async () => {
      if (localSession) {
        await refresh(localSession)
      }
    },
  }

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}

export function useSession() {
  const context = useContext(SessionContext)
  if (!context) {
    throw new Error('useSession must be used inside SessionProvider')
  }

  return context
}
