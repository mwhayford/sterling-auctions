import { FullConfig } from '@playwright/test';

/**
 * Global teardown for Playwright tests
 * This runs once after all tests complete
 */
async function globalTeardown(config: FullConfig) {
  console.log('🧹 Starting global teardown...');
  
  // Clean up any global resources
  // For example: close database connections, clean up test data, etc.
  
  console.log('✅ Global teardown completed');
}

export default globalTeardown;
