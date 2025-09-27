import { defineConfig, devices } from '@playwright/test';

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
// require('dotenv').config();

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: './specs',
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'test-results/results.json' }],
    ['junit', { outputFile: 'test-results/results.xml' }],
    ['line']
  ],
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    baseURL: process.env.BASE_URL || 'http://localhost:3000',
    
    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on-first-retry',
    
    /* Take screenshot only when test fails */
    screenshot: 'only-on-failure',
    
    /* Record video only when test fails */
    video: 'retain-on-failure',
    
    /* Global timeout for all tests */
    actionTimeout: 30000,
    navigationTimeout: 30000,
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },

    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },

    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },

    /* Test against mobile viewports. */
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    },

    /* Test against branded browsers. */
    {
      name: 'Microsoft Edge',
      use: { ...devices['Desktop Edge'], channel: 'msedge' },
    },
    {
      name: 'Google Chrome',
      use: { ...devices['Desktop Chrome'], channel: 'chrome' },
    },
  ],

  /* Global setup and teardown */
  globalSetup: require.resolve('./global-setup.ts'),
  globalTeardown: require.resolve('./global-teardown.ts'),

  /* Run your local dev server before starting the tests */
  webServer: [
    {
      command: 'npm start',
      cwd: '../../src/frontend',
      port: 3000,
      reuseExistingServer: !process.env.CI,
      timeout: 120000,
    },
    {
      command: 'dotnet run --project SterlingAuctions.API.csproj',
      cwd: '../../src/backend',
      port: 5000,
      reuseExistingServer: !process.env.CI,
      timeout: 120000,
    }
  ],

  /* Expect options */
  expect: {
    /* Timeout for assertions */
    timeout: 10000,
    
    /* Custom matchers */
    toHaveScreenshot: {
      threshold: 0.2,
      mode: 'strict'
    },
    
    toMatchSnapshot: {
      threshold: 0.2
    }
  },

  /* Output directory for test artifacts */
  outputDir: 'test-results/',

  /* Maximum time one test can run for. */
  timeout: 60000,
});
