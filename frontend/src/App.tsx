import { Navigate, RouterProvider, createBrowserRouter } from 'react-router-dom'
import { AppLayout } from './app/AppLayout'
import { ProtectedRoute } from './app/ProtectedRoute'
import { AnalyticsPage } from './pages/AnalyticsPage'
import { BoardPage } from './pages/BoardPage'
import { CallbackPage } from './pages/CallbackPage'
import { LoginPage } from './pages/LoginPage'
import { TaskDetailPage } from './pages/TaskDetailPage'
import { WorkspacePage } from './pages/WorkspacePage'

const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/workspace" replace />,
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/callback',
    element: <CallbackPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppLayout />,
        children: [
          {
            path: '/workspace',
            element: <WorkspacePage />,
          },
          {
            path: '/board',
            element: <BoardPage />,
          },
          {
            path: '/tasks/:taskId',
            element: <TaskDetailPage />,
          },
          {
            path: '/analytics',
            element: (
              <ProtectedRoute requiredRole="Manager">
                <AnalyticsPage />
              </ProtectedRoute>
            ),
          },
        ],
      },
    ],
  },
])

export default function App() {
  return <RouterProvider router={router} />
}
