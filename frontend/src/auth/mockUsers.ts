import type { LocalUserSession } from '../api/types'

export const demoUsers: LocalUserSession[] = [
  {
    userId: 'user-manager',
    displayName: 'Красногурський Андрій',
    role: 'Manager',
  },
  {
    userId: 'user-lead',
    displayName: 'Капарис Андрій',
    role: 'Member',
  },
  {
    userId: 'user-backend',
    displayName: 'Богдан',
    role: 'Member',
  },
  {
    userId: 'user-guest',
    displayName: 'Stakeholder Viewer',
    role: 'Guest',
  },
]
