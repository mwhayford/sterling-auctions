import { test, expect } from '@playwright/test';
import { TestUtils } from '../utils/test-utils';
import { testData } from '../fixtures/test-data';

test.describe('Authentication', () => {
  let testUtils: TestUtils;

  test.beforeEach(async ({ page }) => {
    testUtils = new TestUtils(page);
  });

  test('should display login page', async ({ page }) => {
    await page.goto('/login');
    await testUtils.waitForPageLoad();

    // Check if login page elements are visible
    await expect(page.locator(testData.selectors.pages.login)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.login.email)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.login.password)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.login.submit)).toBeVisible();
  });

  test('should display register page', async ({ page }) => {
    await page.goto('/register');
    await testUtils.waitForPageLoad();

    // Check if register page elements are visible
    await expect(page.locator(testData.selectors.pages.register)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.register.firstName)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.register.lastName)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.register.email)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.register.password)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.register.confirmPassword)).toBeVisible();
    await expect(page.locator(testData.selectors.forms.register.submit)).toBeVisible();
  });

  test('should validate login form', async ({ page }) => {
    await page.goto('/login');
    await testUtils.waitForPageLoad();

    // Try to submit empty form
    await page.click(testData.selectors.forms.login.submit);

    // Check for validation messages
    await expect(page.locator('text=' + testData.messages.validation.required)).toBeVisible();
  });

  test('should validate register form', async ({ page }) => {
    await page.goto('/register');
    await testUtils.waitForPageLoad();

    // Try to submit empty form
    await page.click(testData.selectors.forms.register.submit);

    // Check for validation messages
    await expect(page.locator('text=' + testData.messages.validation.required)).toBeVisible();
  });

  test('should show password validation', async ({ page }) => {
    await page.goto('/register');
    await testUtils.waitForPageLoad();

    // Enter invalid password
    await testUtils.fillField(testData.selectors.forms.register.password, '123');
    await testUtils.fillField(testData.selectors.forms.register.confirmPassword, '123');

    // Check for password validation message
    await expect(page.locator('text=' + testData.messages.validation.password)).toBeVisible();
  });

  test('should show email validation', async ({ page }) => {
    await page.goto('/register');
    await testUtils.waitForPageLoad();

    // Enter invalid email
    await testUtils.fillField(testData.selectors.forms.register.email, 'invalid-email');

    // Check for email validation message
    await expect(page.locator('text=' + testData.messages.validation.email)).toBeVisible();
  });

  test('should navigate between login and register', async ({ page }) => {
    await page.goto('/login');
    await testUtils.waitForPageLoad();

    // Click register link
    await page.click('text=Create Account');

    // Should navigate to register page
    await expect(page.locator(testData.selectors.pages.register)).toBeVisible();

    // Click login link
    await page.click('text=Sign In');

    // Should navigate back to login page
    await expect(page.locator(testData.selectors.pages.login)).toBeVisible();
  });
});
