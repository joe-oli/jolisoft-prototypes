import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  workers: 1,
  use: { baseURL: 'http://localhost:6174', channel: 'msedge', headless: true, trace: 'retain-on-failure' },
  webServer: {
    command: 'npm run preview -- --host localhost --port 6174 --strictPort',
    url: 'http://localhost:6174',
    reuseExistingServer: false,
  },
})
