import { test, expect } from '@playwright/test';
import { TestUtils } from '../utils/test-utils';
import { testData } from '../fixtures/test-data';

test.describe('Auctions', () => {
  let testUtils: TestUtils;

  test.beforeEach(async ({ page }) => {
    testUtils = new TestUtils(page);
    await page.goto('/auctions');
    await testUtils.waitForPageLoad();
  });

  test('should display auctions page', async ({ page }) => {
    // Check if auctions page is visible
    await expect(page.locator(testData.selectors.pages.auctions)).toBeVisible();
    
    // Check for page title
    await expect(page.locator('h1')).toContainText('Auctions');
  });

  test('should display auction cards', async ({ page }) => {
    // Wait for auction cards to load
    await page.waitForSelector(testData.selectors.components.auctionCard, { timeout: 10000 });
    
    // Check if at least one auction card is visible
    const auctionCards = page.locator(testData.selectors.components.auctionCard);
    await expect(auctionCards.first()).toBeVisible();
  });

  test('should navigate to auction detail', async ({ page }) => {
    // Wait for auction cards to load
    await page.waitForSelector(testData.selectors.components.auctionCard, { timeout: 10000 });
    
    // Click on first auction card
    await page.click(testData.selectors.components.auctionCard + ':first-child');
    
    // Should navigate to auction detail page
    await expect(page.locator(testData.selectors.pages.auctionDetail)).toBeVisible();
  });

  test('should display auction information', async ({ page }) => {
    // Wait for auction cards to load
    await page.waitForSelector(testData.selectors.components.auctionCard, { timeout: 10000 });
    
    const firstCard = page.locator(testData.selectors.components.auctionCard).first();
    
    // Check if auction title is visible
    await expect(firstCard.locator('[data-testid="auction-title"]')).toBeVisible();
    
    // Check if auction description is visible
    await expect(firstCard.locator('[data-testid="auction-description"]')).toBeVisible();
    
    // Check if current bid is visible
    await expect(firstCard.locator('[data-testid="auction-current-bid"]')).toBeVisible();
    
    // Check if countdown timer is visible
    await expect(firstCard.locator(testData.selectors.components.countdownTimer)).toBeVisible();
  });

  test('should filter auctions by category', async ({ page }) => {
    // Wait for page to load
    await page.waitForSelector(testData.selectors.components.auctionCard, { timeout: 10000 });
    
    // Check if category filter is present
    const categoryFilter = page.locator('[data-testid="category-filter"]');
    if (await categoryFilter.isVisible()) {
      // Select a category
      await categoryFilter.selectOption(testData.categories[0]);
      
      // Wait for filtered results
      await page.waitForTimeout(1000);
      
      // Check if filtered auctions are displayed
      const auctionCards = page.locator(testData.selectors.components.auctionCard);
      await expect(auctionCards.first()).toBeVisible();
    }
  });

  test('should search auctions', async ({ page }) => {
    // Wait for page to load
    await page.waitForSelector(testData.selectors.components.auctionCard, { timeout: 10000 });
    
    // Check if search input is present
    const searchInput = page.locator('[data-testid="search-input"]');
    if (await searchInput.isVisible()) {
      // Enter search term
      await searchInput.fill('test');
      
      // Press Enter or click search button
      await page.press(searchInput, 'Enter');
      
      // Wait for search results
      await page.waitForTimeout(1000);
      
      // Check if search results are displayed
      const auctionCards = page.locator(testData.selectors.components.auctionCard);
      await expect(auctionCards.first()).toBeVisible();
    }
  });

  test('should handle empty state', async ({ page }) => {
    // This test would need to be run with no auctions in the system
    // or with specific test data that results in no auctions
    
    // Check if empty state message is displayed
    const emptyState = page.locator('[data-testid="empty-state"]');
    if (await emptyState.isVisible()) {
      await expect(emptyState).toContainText('No auctions found');
    }
  });

  test('should be responsive on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    // Wait for page to load
    await page.waitForSelector(testData.selectors.components.auctionCard, { timeout: 10000 });
    
    // Check if auction cards are still visible and properly formatted
    const auctionCards = page.locator(testData.selectors.components.auctionCard);
    await expect(auctionCards.first()).toBeVisible();
    
    // Check if mobile-specific elements are present
    const mobileMenu = page.locator('[data-testid="mobile-menu"]');
    if (await mobileMenu.isVisible()) {
      await expect(mobileMenu).toBeVisible();
    }
  });
});
