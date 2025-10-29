# PoDebateRap E2E Tests

TypeScript Playwright E2E test suite for the PoDebateRap application.

## Overview

This test suite provides comprehensive end-to-end testing for all main UI functionality using Playwright and TypeScript. Tests are executed on **Chromium only** for both desktop and mobile viewports.

## Prerequisites

- Node.js 18+ and npm
- .NET 9.0 SDK
- PoDebateRap server running on `http://localhost:5000`

## Installation

```bash
cd Tests/PoDebateRap.E2ETests
npm install
npx playwright install chromium
```

## Running Tests

### All Tests
```bash
npm test
```

### Headed Mode (See Browser)
```bash
npm run test:headed
```

### Debug Mode
```bash
npm run test:debug
```

### Interactive UI Mode
```bash
npm run test:ui
```

### Desktop Tests Only
```bash
npm run test:desktop
```

### Mobile Tests Only
```bash
npm run test:mobile
```

### View Test Report
```bash
npm run test:report
```

## Test Structure

```
tests/
├── helpers/
│   └── page-objects.ts       # Page Object Models and test data
├── debate-setup.spec.ts       # Debate setup form validation tests
├── debate-flow.spec.ts        # Full debate flow E2E tests
├── audio-playback.spec.ts     # Audio generation and playback tests
└── diagnostics.spec.ts        # Diagnostics page health check tests
```

## Test Coverage

### Debate Setup (`debate-setup.spec.ts`)
- ✅ Form validation (required fields, same rapper prevention)
- ✅ Dynamic button enabling/disabling
- ✅ User input handling
- ✅ Error message display
- ✅ Mobile responsiveness
- ✅ Touch interactions

### Debate Flow (`debate-flow.spec.ts`)
- ✅ Debate setup validation
- ✅ Begin debate button enabling
- ✅ Form validation
- ✅ Navigation between pages

**Note**: Long-running tests that wait for AI verse generation have been removed to keep suite fast.

### Audio Playback (`audio-playback.spec.ts`)
- ✅ Audio player UI element verification
- ✅ Console error checking for audio/media

**Note**: Tests that generate audio via Azure AI have been removed to keep suite under 1 minute.

### Diagnostics (`diagnostics.spec.ts`)
- ✅ Health check page display
- ✅ Azure OpenAI service status
- ✅ Azure Speech service status
- ✅ Azure Storage service status
- ✅ News API service status
- ✅ Page navigation

## Test Configuration

### Viewports

**Desktop (Chromium)**
- Width: 1280px
- Height: 720px
- Project name: `chromium-desktop`

**Mobile (Chromium - Pixel 5)**
- Width: 393px
- Height: 851px
- Touch enabled
- Project name: `chromium-mobile`

### Timeouts

- Default test timeout: 60 seconds
- Server startup timeout: 120 seconds

**Performance Target**: Full E2E suite completes in **under 1 minute**

Long-running tests involving AI verse generation and audio playback (1.5-3+ minutes each) have been
removed from the E2E suite and are covered by integration tests instead.

## Page Object Model

All tests use the Page Object Model pattern for maintainability:

- `DebateSetupPage` - Debate setup screen interactions
- `DebateArenaPage` - Debate arena interactions
- `DiagnosticsPage` - Diagnostics page interactions
- `TestData` - Centralized test data (rappers, topics, wait times)

## Test Tags

Tests are tagged for selective execution:

- `@desktop` - Desktop viewport tests
- `@mobile` - Mobile viewport tests

Example:
```bash
# Run only desktop tests
npx playwright test --grep @desktop

# Run only mobile tests
npx playwright test --grep @mobile
```

## CI/CD Integration

The test suite is designed for CI/CD with:

- Automatic server startup via `webServer` configuration
- Retry logic (2 retries in CI)
- Screenshot on failure
- Video recording on failure
- Trace collection on first retry

### GitHub Actions Example

```yaml
- name: Install dependencies
  run: |
    cd Tests/PoDebateRap.E2ETests
    npm ci

- name: Install Playwright Browsers
  run: npx playwright install chromium

- name: Run E2E Tests
  run: |
    cd Tests/PoDebateRap.E2ETests
    npm test

- name: Upload test results
  if: always()
  uses: actions/upload-artifact@v3
  with:
    name: playwright-report
    path: Tests/PoDebateRap.E2ETests/playwright-report/
```

## Debugging

### Record Test
```bash
npm run codegen
```

### View Trace
```bash
npx playwright show-trace test-results/trace.zip
```

### Headed Mode with Slow Motion
Edit `playwright.config.ts`:
```typescript
use: {
  headless: false,
  launchOptions: {
    slowMo: 1000  // Slow down by 1 second
  }
}
```

## Known Limitations

1. **Server Dependency**: Tests require the PoDebateRap server to be running or will auto-start it
2. **Azure Services**: Some tests expect Azure services to be configured (OpenAI, Speech, Storage)
3. **Network Delays**: Audio generation tests have extended timeouts due to AI processing
4. **Chromium Only**: Tests are optimized for Chromium; Firefox/WebKit are not included

## Troubleshooting

### Tests Timing Out
- Increase timeout in test: `test.setTimeout(120000)`
- Check if server is running on port 5000
- Verify Azure services are configured with valid credentials

### Element Not Found
- Check if element IDs match the Blazor components
- Wait for Blazor to initialize (default 2 second delay)
- Use `page.waitForLoadState('networkidle')`

### Audio Tests Failing
- Ensure Azure Speech service is configured
- Check network connectivity
- Verify audio codec support in test environment

## Contributing

When adding new tests:

1. Use Page Object Model pattern
2. Add appropriate `@desktop` or `@mobile` tags
3. Set realistic timeouts for AI operations
4. Include both positive and negative test cases
5. Test on both viewports where applicable

## License

Same as parent PoDebateRap project.
