import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { useSession } from '../auth/SessionContext'

type RealtimeContextValue = {
  taskRevision: number
  notificationRevision: number
}

const RealtimeContext = createContext<RealtimeContextValue | undefined>(undefined)

export function RealtimeProvider({ children }: { children: React.ReactNode }) {
  const { localSession, serverSession } = useSession()
  const [taskRevision, setTaskRevision] = useState(0)
  const [notificationRevision, setNotificationRevision] = useState(0)

  useEffect(() => {
    if (!localSession || !serverSession?.workspaceId) {
      return
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`/hubs/codebasky?workspaceId=${serverSession.workspaceId}&userId=${localSession.userId}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('taskChanged', () => setTaskRevision((value) => value + 1))
    connection.on('commentAdded', () => {
      setTaskRevision((value) => value + 1)
      setNotificationRevision((value) => value + 1)
    })
    connection.on('notificationChanged', () => setNotificationRevision((value) => value + 1))

    void connection.start()

    return () => {
      void connection.stop()
    }
  }, [localSession, serverSession?.workspaceId])

  const value = useMemo(
    () => ({ taskRevision, notificationRevision }),
    [notificationRevision, taskRevision],
  )

  return <RealtimeContext.Provider value={value}>{children}</RealtimeContext.Provider>
}

export function useRealtime() {
  const context = useContext(RealtimeContext)
  if (!context) {
    throw new Error('useRealtime must be used inside RealtimeProvider')
  }

  return context
}
