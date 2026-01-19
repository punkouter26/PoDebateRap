import { test, expect, Page } from '@playwright/test';

/**
 * Rapper Selection E2E Tests
 * Tests for the rapper dropdown selection functionality
 * 
 * Uses Radzen Blazor dropdown components with specific selectors
 */

// Helper to wait for Blazor WASM to fully load
async function waitForBlazorReady(page: Page) {
  await page.waitForLoadState('networkidle');
  // Wait for Blazor to initialize and rappers to load from API
  await page.waitForFunction(() => {
    const dropdowns = document.querySelectorAll('.rapper-dropdown');
    return dropdowns.length >= 2;
  }, { timeout: 15000 });
  await page.waitForTimeout(1000); // Extra buffer for WASM components
}

// Helper to click dropdown and wait for popup
async function openDropdownAndWaitForItems(page: Page, dropdownSelector: string) {
  const dropdownWrapper = page.locator(dropdownSelector);
  
  // Wait for dropdown wrapper to be visible
  await dropdownWrapper.waitFor({ state: 'visible', timeout: 15000 });
  
  // The actual Radzen dropdown trigger is the .rz-dropdown element inside the wrapper
  const dropdownTrigger = dropdownWrapper.locator('.rz-dropdown');
  await dropdownTrigger.waitFor({ state: 'visible', timeout: 10000 });
  
  // Wait for the dropdown to not be disabled (data loaded)
  await expect(dropdownTrigger).not.toHaveClass(/rz-state-disabled/, { timeout: 15000 });
  
  // Click on the actual dropdown trigger to open it
  await dropdownTrigger.click();
  
  // Wait for dropdown panel to appear and have visible items
  const panel = page.locator('.rz-dropdown-panel').last();
  await panel.waitFor({ state: 'visible', timeout: 10000 });
  
  // Wait for items to be visible in the panel
  await panel.locator('.rz-dropdown-item').first().waitFor({ state: 'visible', timeout: 10000 });
  await page.waitForTimeout(300);
}

// Helper to get visible dropdown items (from the last/most recent dropdown panel)
function getVisibleDropdownItems(page: Page) {
  // Radzen appends dropdown panels to body, the last one is the currently open one
  return page.locator('.rz-dropdown-panel').last().locator('.rz-dropdown-item');
}

test.describe('Rapper Selection Dropdowns', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await waitForBlazorReady(page);
  });

  test('should display rapper dropdown selectors @desktop', async ({ page }) => {
    // Wait for dropdowns to be visible
    const rapper1Dropdown = page.locator('.rapper-dropdown').first();
    const rapper2Dropdown = page.locator('.rapper-dropdown').nth(1);
    
    await expect(rapper1Dropdown).toBeVisible({ timeout: 10000 });
    await expect(rapper2Dropdown).toBeVisible({ timeout: 10000 });
  });

  test('should open rapper 1 dropdown and show options @desktop', async ({ page }) => {
    // Find and click the first dropdown
    await openDropdownAndWaitForItems(page, '.rapper-dropdown >> nth=0');
    
    // Check that dropdown items are visible
    const dropdownItems = getVisibleDropdownItems(page);
    const count = await dropdownItems.count();
    expect(count).toBeGreaterThan(0);
  });

  test('should select rapper from dropdown @desktop', async ({ page }) => {
    const rapper1Dropdown = page.locator('.rapper-dropdown').first();
    
    // Open dropdown and select first option
    await openDropdownAndWaitForItems(page, '.rapper-dropdown >> nth=0');
    
    // Get and click the first visible option
    const firstOption = getVisibleDropdownItems(page).first();
    const rapperName = await firstOption.textContent();
    await firstOption.click();
    await page.waitForTimeout(500);
    
    // Verify selection was made - dropdown should show the selected rapper's name
    await expect(rapper1Dropdown).toContainText(rapperName || '');
  });

  test('should not show selected rapper 1 in rapper 2 dropdown @desktop', async ({ page }) => {
    // Open first dropdown and select first rapper
    await openDropdownAndWaitForItems(page, '.rapper-dropdown >> nth=0');
    
    const firstOption = getVisibleDropdownItems(page).first();
    const selectedRapperName = (await firstOption.textContent())?.trim();
    await firstOption.click();
    await page.waitForTimeout(500);
    
    // Open second dropdown
    await openDropdownAndWaitForItems(page, '.rapper-dropdown >> nth=1');
    
    // Get all rapper names in the second dropdown
    const rapper2Options = getVisibleDropdownItems(page);
    const rapper2Texts = await rapper2Options.allTextContents();
    
    // The first rapper should NOT appear in the second dropdown
    const hasSelectedRapper = rapper2Texts.some(text => 
      text.trim() === selectedRapperName
    );
    expect(hasSelectedRapper).toBe(false);
  });

  test('should enable Begin Debate when both rappers and topic selected @desktop', async ({ page }) => {
    // Select rapper 1
    await openDropdownAndWaitForItems(page, '.rapper-dropdown >> nth=0');
    await getVisibleDropdownItems(page).first().click();
    await page.waitForTimeout(500);
    
    // Select rapper 2
    await openDropdownAndWaitForItems(page, '.rapper-dropdown >> nth=1');
    await getVisibleDropdownItems(page).first().click();
    await page.waitForTimeout(500);
    
    // Enter a topic - find the Radzen text input
    const topicInput = page.locator('.rz-textbox, input[placeholder*="topic" i]').first();
    await topicInput.fill('Test Topic for E2E');
    await page.waitForTimeout(500);
    
    // Begin Debate button should be enabled
    const beginButton = page.getByRole('button', { name: /begin debate/i });
    await expect(beginButton).toBeEnabled({ timeout: 5000 });
  });

  test('should show VS badge between dropdowns @desktop', async ({ page }) => {
    // Look for VS text in the selection row area
    const vsBadge = page.locator('.vs-badge').or(page.locator('.selection-row >> text=VS'));
    await expect(vsBadge.first()).toBeVisible({ timeout: 5000 });
  });

  test('should allow filtering rappers in dropdown @desktop', async ({ page }) => {
    // Open first dropdown
    await openDropdownAndWaitForItems(page, '.rapper-dropdown >> nth=0');
    
    // Look for the filter input in the visible dropdown panel
    const filterInput = page.locator('.rz-dropdown-panel:visible .rz-dropdown-filter');
    
    if (await filterInput.isVisible()) {
      // Get initial count
      const initialCount = await getVisibleDropdownItems(page).count();
      
      // Type to filter
      await filterInput.fill('Em');
      await page.waitForTimeout(500);
      
      // Should filter results (fewer or equal items)
      const filteredCount = await getVisibleDropdownItems(page).count();
      expect(filteredCount).toBeLessThanOrEqual(initialCount);
      expect(filteredCount).toBeGreaterThan(0); // Should have at least Eminem
    } else {
      // If no filter input, just verify dropdown works
      const count = await getVisibleDropdownItems(page).count();
      expect(count).toBeGreaterThan(0);
    }
  });
});

test.describe('Rapper Selection - Mobile', () => {
  test.use({ viewport: { width: 375, height: 667 } });

  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000);
  });

  test('should display rapper dropdowns on mobile @mobile', async ({ page }) => {
    const rapper1Dropdown = page.locator('.rapper-dropdown').first();
    const rapper2Dropdown = page.locator('.rapper-dropdown').nth(1);
    
    await expect(rapper1Dropdown).toBeVisible({ timeout: 10000 });
    await expect(rapper2Dropdown).toBeVisible({ timeout: 10000 });
  });

  test('should stack dropdowns vertically on mobile @mobile', async ({ page }) => {
    const selectionRow = page.locator('.selection-row');
    
    // On mobile, the flex direction should be column
    const flexDirection = await selectionRow.evaluate(el => 
      window.getComputedStyle(el).flexDirection
    );
    
    expect(flexDirection).toBe('column');
  });
});
