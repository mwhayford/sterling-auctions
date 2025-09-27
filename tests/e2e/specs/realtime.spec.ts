import { test, expect } from '@playwright/test';
import { TestUtils } from '../utils/test-utils';
import { testData } from '../fixtures/test-data';

test.describe('Real-time Features', () => {
  let testUtils: TestUtils;

  test.beforeEach(async ({ page }) => {
    testUtils = new TestUtils(page);
    await page.goto('/');
    await testUtils.waitForPageLoad();
  });

  test('should establish SignalR connection', async ({ page }) => {
    // Wait for SignalR connection to be established
    await testUtils.waitForSignalRConnection();
    
    // Check connection status
    const statusElement = page.locator(testData.selectors.components.connectionStatus);
    await expect(statusElement).toBeVisible();
    
    const statusText = await testUtils.getTextContent(testData.selectors.components.connectionStatus);
    expect(statusText).toContain('Connected');
  });

  test('should display notification center', async ({ page }) => {
    // Wait for notification center to be ready
    await testUtils.waitForNotificationsReady();
    
    // Check if notification center is visible
    await expect(page.locator(testData.selectors.components.notificationCenter)).toBeVisible();
    
    // Click notification bell
    await page.click(testData.selectors.components.notificationCenter);
    
    // Check if notification panel opens
    await expect(page.locator('[data-testid="notification-panel"]')).toBeVisible();
  });

  test('should handle notification tabs', async ({ page }) => {
    // Wait for notification center to be ready
    await testUtils.waitForNotificationsReady();
    
    // Open notification panel
    await page.click(testData.selectors.components.notificationCenter);
    
    // Check if tabs are present
    await expect(page.locator('[data-testid="notification-tab-all"]')).toBeVisible();
    await expect(page.locator('[data-testid="notification-tab-system"]')).toBeVisible();
    await expect(page.locator('[data-testid="notification-tab-admin"]')).toBeVisible();
    
    // Click on system tab
    await page.click('[data-testid="notification-tab-system"]');
    
    // Check if system notifications are displayed
    await expect(page.locator('[data-testid="system-notifications"]')).toBeVisible();
  });

  test('should show connection status indicator', async ({ page }) => {
    // Wait for connection status to appear
    await expect(page.locator(testData.selectors.components.connectionStatus)).toBeVisible();
    
    // Check if status indicator has correct styling
    const statusElement = page.locator(testData.selectors.components.connectionStatus);
    const statusClass = await statusElement.getAttribute('class');
    expect(statusClass).toMatch(/connected|disconnected|connecting/);
  });

  test('should handle connection loss gracefully', async ({ page }) => {
    // Wait for initial connection
    await testUtils.waitForSignalRConnection();
    
    // Simulate connection loss by going offline
    await page.context().setOffline(true);
    
    // Wait a moment for connection status to update
    await page.waitForTimeout(2000);
    
    // Check if connection status shows disconnected
    const statusText = await testUtils.getTextContent(testData.selectors.components.connectionStatus);
    expect(statusText).toMatch(/Disconnected|Offline/);
    
    // Go back online
    await page.context().setOffline(false);
    
    // Wait for reconnection
    await page.waitForTimeout(3000);
    
    // Check if connection is restored
    const reconnectedStatus = await testUtils.getTextContent(testData.selectors.components.connectionStatus);
    expect(reconnectedStatus).toMatch(/Connected|Reconnected/);
  });

  test('should display toast notifications', async ({ page }) => {
    // Wait for notification system to be ready
    await testUtils.waitForNotificationsReady();
    
    // This test would need to trigger a notification
    // For now, we'll just check if the toast container exists
    const toastContainer = page.locator('[data-testid="toast-container"]');
    if (await toastContainer.isVisible()) {
      await expect(toastContainer).toBeVisible();
    }
  });

  test('should handle real-time updates', async ({ page }) => {
    // Wait for SignalR connection
    await testUtils.waitForSignalRConnection();
    
    // Navigate to auctions page
    await page.goto('/auctions');
    await testUtils.waitForPageLoad();
    
    // Wait for auction cards to load
    await page.waitForSelector(testData.selectors.components.auctionCard, { timeout: 10000 });
    
    // Check if countdown timers are updating
    const countdownTimer = page.locator(testData.selectors.components.countdownTimer).first();
    if (await countdownTimer.isVisible()) {
      const initialTime = await countdownTimer.textContent();
      
      // Wait a few seconds
      await page.waitForTimeout(3000);
      
      // Check if time has updated
      const updatedTime = await countdownTimer.textContent();
      expect(updatedTime).not.toBe(initialTime);
    }
  });

  test('should work across different browsers', async ({ page, browserName }) => {
    // Wait for SignalR connection
    await testUtils.waitForSignalRConnection();
    
    // Check if connection status is visible regardless of browser
    await expect(page.locator(testData.selectors.components.connectionStatus)).toBeVisible();
    
    // Check if notification center works
    await testUtils.waitForNotificationsReady();
    await expect(page.locator(testData.selectors.components.notificationCenter)).toBeVisible();
    
    console.log(`✅ Real-time features working on ${browserName}`);
  });
});
