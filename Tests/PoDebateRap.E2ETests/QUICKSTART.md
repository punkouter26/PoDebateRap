# Quick Start: TypeScript E2E Tests

## Setup (One-time)

```bash
# Navigate to E2E test directory
cd Tests/PoDebateRap.E2ETests

# Install dependencies
npm install

# Install Playwright Chromium browser
npx playwright install chromium
```

## Run Tests

### Option 1: Auto-start Server (Recommended)
The tests will automatically start the server if it's not running:

```bash
npm test
```

### Option 2: Manual Server Start
If you prefer to start the server manually:

```bash
# Terminal 1: Start server
cd ../..
dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# Terminal 2: Run tests
cd Tests/PoDebateRap.E2ETests
npm test
```

## Common Commands

```bash
# Run all tests
npm test

# Run tests in headed mode (see browser)
npm run test:headed

# Run with interactive UI
npm run test:ui

# Run only desktop tests
npm run test:desktop

# Run only mobile tests
npm run test:mobile

# Debug tests
npm run test:debug

# View last test report
npm run test:report

# Generate test code (record actions)
npm run codegen
```

## Test Results

After running tests, view the HTML report:

```bash
npm run test:report
```

Or open manually:
```
playwright-report/index.html
```

## Test Structure

```
tests/
├── debate-setup.spec.ts      # 12 tests - Form validation
├── debate-flow.spec.ts        # 8 tests - Complete flows
├── audio-playback.spec.ts     # 10 tests - Audio functionality
├── diagnostics.spec.ts        # 8 tests - Health checks
└── helpers/
    └── page-objects.ts        # Page Object Models
```

## Filtering Tests

Run specific test files:
```bash
npx playwright test debate-setup
npx playwright test audio-playback
```

Run specific test names:
```bash
npx playwright test -g "should enable Begin Debate button"
```

Run by project (viewport):
```bash
npx playwright test --project chromium-desktop
npx playwright test --project chromium-mobile
```

## Troubleshooting

### Tests timeout waiting for elements
**Problem**: `Timeout 30000ms exceeded. Call log: - waiting for Locator("#rapper1Select")`

**Solution**: Server is not running on localhost:5000. Either:
1. Let auto-start handle it (may take 2 minutes first time)
2. Start server manually before running tests

### Chromium not installed
**Problem**: `Executable doesn't exist at C:\Users\...\ms-playwright\chromium-...`

**Solution**:
```bash
npx playwright install chromium
```

### Tests fail with "element not found"
**Problem**: Blazor components haven't initialized

**Solution**: Wait times are configured in `page-objects.ts`. If still failing, increase:
```typescript
waitTimes: {
  blazorInit: 2000,  // Increase this if needed
}
```

## CI/CD Integration

The tests are configured for CI/CD with auto-retry and artifact upload:

```yaml
- name: Run E2E Tests
  run: |
    cd Tests/PoDebateRap.E2ETests
    npm ci
    npx playwright install chromium
    npm test

- name: Upload Test Results
  if: always()
  uses: actions/upload-artifact@v3
  with:
    name: playwright-report
    path: Tests/PoDebateRap.E2ETests/playwright-report/
```

## Next Steps

1. Run tests: `npm test`
2. View report: `npm run test:report`
3. Explore interactive UI: `npm run test:ui`
4. Read full docs: `README.md`
