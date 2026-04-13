export type WorkspaceRole = 'Manager' | 'Member' | 'Guest'
export type WorkItemStatus = 'Todo' | 'InProgress' | 'Done'
export type WorkItemPriority = 'Low' | 'Medium' | 'High' | 'Critical'
export type NotificationType = 'Assignment' | 'Mention' | 'DueSoon' | 'StatusChange' | 'Comment'

export type LocalUserSession = {
  userId: string
  displayName: string
  role: WorkspaceRole
}

export type SessionDto = {
  userId: string
  displayName: string
  role: WorkspaceRole
  workspaceId: string
  workspaceName: string
}

export type WorkspaceMemberDto = {
  userId: string
  displayName: string
  role: WorkspaceRole
}

export type ProjectSummaryDto = {
  id: string
  name: string
  summary: string
  status: string
  openTasks: number
}

export type WorkspaceOverviewDto = {
  workspaceId: string
  name: string
  description: string
  members: WorkspaceMemberDto[]
  projects: ProjectSummaryDto[]
  openTasks: number
  dueThisWeek: number
}

export type TaskSummaryDto = {
  id: string
  projectId: string
  projectName: string
  title: string
  description: string
  status: WorkItemStatus
  priority: WorkItemPriority
  assigneeUserId: string | null
  assigneeDisplayName: string | null
  dueDateUtc: string | null
  requirementKey: string | null
  isOverdue: boolean
  updatedAtUtc: string
}

export type TaskActivityDto = {
  id: string
  actorDisplayName: string
  message: string
  createdAtUtc: string
}

export type TaskCommentDto = {
  id: string
  authorDisplayName: string
  body: string
  createdAtUtc: string
}

export type TaskDetailsDto = {
  id: string
  projectId: string
  projectName: string
  title: string
  description: string
  status: WorkItemStatus
  priority: WorkItemPriority
  assigneeUserId: string | null
  assigneeDisplayName: string | null
  dueDateUtc: string | null
  requirementKey: string | null
  activities: TaskActivityDto[]
  comments: TaskCommentDto[]
}

export type AnalyticsBarDto = {
  label: string
  value: number
}

export type RiskItemDto = {
  title: string
  detail: string
}

export type OverdueFocusDto = {
  taskId: string
  title: string
  owner: string | null
  requirementKey: string | null
}

export type AnalyticsDto = {
  totalTasks: number
  doneThisSprint: number
  inProgress: number
  overdue: number
  throughput: AnalyticsBarDto[]
  risks: RiskItemDto[]
  overdueFocus: OverdueFocusDto | null
}

export type NotificationDto = {
  id: string
  type: NotificationType
  title: string
  message: string
  taskItemId: string | null
  isRead: boolean
  createdAtUtc: string
}
