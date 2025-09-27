import { test, expect } from '@playwright/test';
import { TestUtils } from '../utils/test-utils';
import { testData } from '../fixtures/test-data';

test.describe('Home Page', () => {
  let testUtils: TestUtils;

  test.beforeEach(async ({ page }) => {
    testUtils = new TestUtils(page);
    await page.goto('/');
    await testUtils.waitForPageLoad();
  });

  test('should load home page successfully', async ({ page }) => {
    // Check if home page elements are visible
    await expect(page.locator(testData.selectors.pages.home)).toBeVisible();
    
    // Check for main heading
    await expect(page.locator('h1')).toContainText('Sterling Auctions');
    
    // Check for navigation
    await expect(page.locator(testData.selectors.nav.home)).toBeVisible();
    await expect(page.locator(testData.selectors.nav.auctions)).toBeVisible();
  });

  test('should display connection status', async ({ page }) => {
    // Wait for connection status to appear
    await expect(page.locator(testData.selectors.components.connectionStatus)).toBeVisible();
    
    // Check connection status text
    const statusText = await testUtils.getTextContent(testData.selectors.components.connectionStatus);
    expect(statusText).toMatch(/Connected|Disconnected|Connecting/);
  });

  test('should show notification center', async ({ page }) => {
    // Check if notification center is present
    await expect(page.locator(testData.selectors.components.notificationCenter)).toBeVisible();
    
    // Click notification bell
    await page.click(testData.selectors.components.notificationCenter);
    
    // Check if notification panel opens
    await expect(page.locator('[data-testid="notification-panel"]')).toBeVisible();
  });

  test('should navigate to auctions page', async ({ page }) => {
    // Click auctions navigation
    await testUtils.clickAndWait(testData.selectors.nav.auctions, testData.selectors.pages.auctions);
    
    // Verify we're on auctions page
    await expect(page.locator(testData.selectors.pages.auctions)).toBeVisible();
  });

  test('should be responsive on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    // Check if mobile navigation is visible
    await expect(page.locator('[data-testid="mobile-menu"]')).toBeVisible();
    
    // Check if content is still accessible
    await expect(page.locator(testData.selectors.pages.home)).toBeVisible();
  });
});
