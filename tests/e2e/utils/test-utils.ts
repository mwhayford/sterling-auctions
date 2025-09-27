import { Page, expect } from '@playwright/test';

/**
 * Test utilities for Sterling Auctions E2E tests
 */
export class TestUtils {
  constructor(private page: Page) {}

  /**
   * Wait for the page to be fully loaded
   */
  async waitForPageLoad() {
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForSelector('body', { state: 'visible' });
  }

  /**
   * Wait for SignalR connection to be established
   */
  async waitForSignalRConnection() {
    // Wait for connection status indicator
    await this.page.waitForSelector('[data-testid="connection-status"]', { timeout: 10000 });
    
    // Check if connection is established
    const connectionStatus = await this.page.locator('[data-testid="connection-status"]').textContent();
    expect(connectionStatus).toContain('Connected');
  }

  /**
   * Wait for notifications to be ready
   */
  async waitForNotificationsReady() {
    await this.page.waitForSelector('[data-testid="notification-center"]', { timeout: 5000 });
  }

  /**
   * Take a screenshot with a descriptive name
   */
  async takeScreenshot(name: string) {
    await this.page.screenshot({ 
      path: `test-results/screenshots/${name}-${Date.now()}.png`,
      fullPage: true 
    });
  }

  /**
   * Fill form field with validation
   */
  async fillField(selector: string, value: string) {
    const field = this.page.locator(selector);
    await field.clear();
    await field.fill(value);
    await field.blur(); // Trigger validation
  }

  /**
   * Click button and wait for navigation or action
   */
  async clickAndWait(selector: string, waitFor?: string) {
    await this.page.click(selector);
    if (waitFor) {
      await this.page.waitForSelector(waitFor);
    }
  }

  /**
   * Wait for API response
   */
  async waitForApiResponse(urlPattern: string | RegExp) {
    const response = await this.page.waitForResponse(urlPattern);
    expect(response.status()).toBeLessThan(400);
    return response;
  }

  /**
   * Check if element is visible and enabled
   */
  async isElementReady(selector: string): Promise<boolean> {
    try {
      const element = this.page.locator(selector);
      await element.waitFor({ state: 'visible', timeout: 5000 });
      return await element.isEnabled();
    } catch {
      return false;
    }
  }

  /**
   * Get text content safely
   */
  async getTextContent(selector: string): Promise<string | null> {
    try {
      const element = this.page.locator(selector);
      await element.waitFor({ state: 'visible', timeout: 5000 });
      return await element.textContent();
    } catch {
      return null;
    }
  }

  /**
   * Wait for toast notification
   */
  async waitForToast(message?: string) {
    const toast = this.page.locator('[data-testid="toast-notification"]');
    await toast.waitFor({ state: 'visible', timeout: 10000 });
    
    if (message) {
      await expect(toast).toContainText(message);
    }
  }

  /**
   * Wait for modal to appear
   */
  async waitForModal() {
    await this.page.waitForSelector('[data-testid="modal"]', { timeout: 10000 });
  }

  /**
   * Close modal
   */
  async closeModal() {
    const closeButton = this.page.locator('[data-testid="modal-close"]');
    if (await closeButton.isVisible()) {
      await closeButton.click();
    }
  }
}
