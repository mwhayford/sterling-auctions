# Playwright End-to-End Testing

This directory contains comprehensive end-to-end tests for the Sterling Auctions platform using Playwright.

## 🚀 Quick Start

```bash
# Install dependencies
npm install

# Install Playwright browsers
npx playwright install

# Run all tests
npm test

# Run tests in headed mode (see browser)
npm run test:headed

# Run tests with UI mode
npm run test:ui

# Run tests in debug mode
npm run test:debug

# Generate and view test report
npm run test:report
```

## 📁 Project Structure

```
tests/e2e/
├── specs/                    # Test specifications
│   ├── home.spec.ts         # Home page tests
│   ├── auth.spec.ts         # Authentication tests
│   ├── auctions.spec.ts     # Auction listing tests
│   └── realtime.spec.ts     # Real-time features tests
├── fixtures/                 # Test data and fixtures
│   └── test-data.ts         # Test data, selectors, and constants
├── utils/                    # Test utilities
│   └── test-utils.ts        # Helper functions and utilities
├── playwright.config.ts     # Playwright configuration
├── global-setup.ts          # Global test setup
├── global-teardown.ts       # Global test cleanup
└── package.json             # Dependencies and scripts
```

## 🧪 Test Categories

### 1. Home Page Tests (`home.spec.ts`)
- ✅ Page loading and navigation
- ✅ Connection status display
- ✅ Notification center functionality
- ✅ Responsive design validation

### 2. Authentication Tests (`auth.spec.ts`)
- ✅ Login page display and validation
- ✅ Registration page display and validation
- ✅ Form validation (email, password, required fields)
- ✅ Navigation between login/register pages

### 3. Auctions Tests (`auctions.spec.ts`)
- ✅ Auction listing page
- ✅ Auction card display
- ✅ Navigation to auction details
- ✅ Category filtering
- ✅ Search functionality
- ✅ Responsive design

### 4. Real-time Features Tests (`realtime.spec.ts`)
- ✅ SignalR connection establishment
- ✅ Notification center functionality
- ✅ Connection status indicators
- ✅ Connection loss handling
- ✅ Real-time updates
- ✅ Cross-browser compatibility

## 🛠️ Test Utilities

### TestUtils Class
The `TestUtils` class provides helper methods for common test operations:

```typescript
const testUtils = new TestUtils(page);

// Wait for page to load
await testUtils.waitForPageLoad();

// Wait for SignalR connection
await testUtils.waitForSignalRConnection();

// Fill form fields with validation
await testUtils.fillField('[data-testid="email"]', 'test@example.com');

// Click and wait for navigation
await testUtils.clickAndWait('[data-testid="submit"]', '[data-testid="success"]');

// Take screenshots
await testUtils.takeScreenshot('test-name');

// Wait for API responses
await testUtils.waitForApiResponse('/api/auctions');
```

### Test Data Fixtures
The `test-data.ts` file contains:
- **User data**: Valid test users and credentials
- **Auction data**: Sample auctions and bids
- **Selectors**: CSS selectors for all UI elements
- **Messages**: Expected success/error messages
- **Timeouts**: Standard timeout values
- **API endpoints**: Backend API routes

## 🎯 Test Selectors

All tests use data-testid attributes for reliable element selection:

```typescript
// Navigation
testData.selectors.nav.home
testData.selectors.nav.auctions
testData.selectors.nav.profile

// Forms
testData.selectors.forms.login.email
testData.selectors.forms.register.firstName
testData.selectors.forms.auction.title

// Components
testData.selectors.components.notificationCenter
testData.selectors.components.connectionStatus
testData.selectors.components.auctionCard
```

## 🌐 Browser Support

Tests run on multiple browsers and devices:
- **Desktop**: Chrome, Firefox, Safari, Edge
- **Mobile**: Chrome Mobile, Safari Mobile
- **Viewports**: Desktop (1920x1080), Mobile (375x667), Tablet (768x1024)

## 📊 Test Reports

Playwright generates comprehensive test reports:

```bash
# Generate HTML report
npm run test:report

# Generate JSON report
npm test -- --reporter=json

# Generate JUnit report
npm test -- --reporter=junit
```

## 🔧 Configuration

### Environment Variables
```bash
# Base URL for tests
BASE_URL=http://localhost:3000

# CI mode (affects retries and workers)
CI=true

# Test timeout
TEST_TIMEOUT=60000
```

### Playwright Configuration
- **Parallel execution**: Tests run in parallel for speed
- **Retry logic**: Failed tests retry 2 times on CI
- **Screenshots**: Captured on failure
- **Videos**: Recorded on failure
- **Traces**: Collected on retry
- **Web servers**: Automatically starts frontend and backend

## 🚦 CI/CD Integration

### GitHub Actions
```yaml
- name: Install Playwright
  run: npx playwright install --with-deps

- name: Run E2E tests
  run: npm test

- name: Upload test results
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: playwright-report
    path: playwright-report/
```

### Docker Support
```dockerfile
# Install Playwright in Docker
RUN npx playwright install --with-deps
RUN npx playwright install chromium
```

## 🐛 Debugging Tests

### Debug Mode
```bash
# Run single test in debug mode
npx playwright test auth.spec.ts --debug

# Run with UI mode
npx playwright test --ui

# Run with trace
npx playwright test --trace on
```

### Screenshots and Videos
- Screenshots are saved to `test-results/screenshots/`
- Videos are saved to `test-results/videos/`
- Traces are saved to `test-results/traces/`

## 📈 Performance Testing

### Load Testing
```bash
# Run performance tests
npx playwright test --grep "performance"

# Monitor network requests
npx playwright test --trace on
```

### Accessibility Testing
```bash
# Run accessibility tests
npx playwright test --grep "accessibility"

# Check for ARIA attributes
await expect(page.locator('[data-testid="button"]')).toHaveAttribute('aria-label');
```

## 🔄 Test Data Management

### Test Data Setup
```typescript
// Use fixtures for consistent test data
const user = testData.users.validUser;
const auction = testData.auctions.validAuction;
```

### Database Seeding
```typescript
// Global setup can seed test data
await seedTestData();
```

## 📝 Best Practices

1. **Use data-testid attributes** for reliable element selection
2. **Wait for elements** before interacting with them
3. **Use page object pattern** for complex pages
4. **Keep tests independent** - no shared state between tests
5. **Use meaningful test names** that describe the scenario
6. **Group related tests** using `test.describe()`
7. **Clean up after tests** using `test.afterEach()`
8. **Use fixtures** for consistent test data
9. **Take screenshots** for debugging failed tests
10. **Test on multiple browsers** for compatibility

## 🚨 Troubleshooting

### Common Issues

1. **Tests timing out**
   - Increase timeout values
   - Check if servers are running
   - Verify network connectivity

2. **Elements not found**
   - Check if data-testid attributes exist
   - Wait for elements to be visible
   - Verify page has loaded completely

3. **SignalR connection issues**
   - Ensure backend is running
   - Check WebSocket support
   - Verify CORS configuration

4. **Cross-browser failures**
   - Check browser-specific selectors
   - Verify CSS compatibility
   - Test responsive design

### Debug Commands
```bash
# Run specific test file
npx playwright test home.spec.ts

# Run specific test
npx playwright test --grep "should load home page"

# Run with verbose output
npx playwright test --verbose

# Run in headed mode
npx playwright test --headed
```

## 📚 Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Best Practices Guide](https://playwright.dev/docs/best-practices)
- [Test Configuration](https://playwright.dev/docs/test-configuration)
- [API Testing](https://playwright.dev/docs/api-testing)
- [Visual Testing](https://playwright.dev/docs/test-snapshots)
