import { expect, test } from '@playwright/test'

async function loginAs(page: import('@playwright/test').Page, name: RegExp) {
  await page.goto('/login')
  await page.getByRole('button', { name }).click()
  await page.getByRole('button', { name: /open workspace/i }).click()
}

test('manager can sign in and see workspace overview', async ({ page }) => {
  await loginAs(page, /красногурський андрій/i)

  await expect(page.getByRole('heading', { name: /codebasky semester team/i })).toBeVisible()
  await expect(page.getByText(/active projects/i)).toBeVisible()
})

test('manager can create a new project from workspace', async ({ page }) => {
  await loginAs(page, /красногурський андрій/i)
  const projectName = `Phase 6 Delivery ${Date.now()}`

  await page.getByLabel(/project name/i).fill(projectName)
  await page.getByLabel(/summary/i).fill('Manual and automated testing rollout')
  await page.getByRole('button', { name: /add project/i }).click()

  await expect(page.locator('.project-card').filter({ hasText: projectName })).toBeVisible()
})

test('member can create a task on the board', async ({ page }) => {
  await loginAs(page, /капарис андрій/i)
  const taskTitle = `Playwright verification task ${Date.now()}`

  await page.getByRole('link', { name: /board/i }).click()
  await page.getByLabel(/^title$/i).fill(taskTitle)
  await page.getByLabel(/^description$/i).fill('Created from end-to-end smoke coverage.')
  await page.getByLabel(/requirement/i).fill('FR-15')
  await page.getByRole('button', { name: /create task/i }).click()

  await expect(
    page
      .locator('.column')
      .filter({ has: page.getByRole('heading', { name: /to do/i }) })
      .getByRole('button', { name: taskTitle }),
  ).toBeVisible()
})

test('member can open task detail and add a comment', async ({ page }) => {
  await loginAs(page, /богдан/i)
  const commentBody = `Playwright smoke comment ${Date.now()}`

  await page.getByRole('link', { name: /board/i }).click()
  await page.getByRole('button', { name: /implement notification center/i }).click()
  await expect(page.getByRole('heading', { name: /implement notification center/i })).toBeVisible()
  await page.getByLabel(/add comment/i).fill(commentBody)
  await page.getByRole('button', { name: /post comment/i }).click()

  await expect(page.locator('.comment-card').filter({ hasText: commentBody })).toHaveCount(1)
})

test('manager can open analytics and see overdue focus', async ({ page }) => {
  await loginAs(page, /красногурський андрій/i)

  await page.getByRole('link', { name: /analytics/i }).click()
  await expect(page.getByRole('heading', { name: /sprint health snapshot/i })).toBeVisible()
  await expect(page.getByText(/overdue focus/i)).toBeVisible()
})
