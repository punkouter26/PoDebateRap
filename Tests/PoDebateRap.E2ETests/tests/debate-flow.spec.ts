import { test, expect } from '@playwright/test';
import { DebateSetupPage, TestData } from './helpers/page-objects';

/**
 * Debate Flow Tests - Streamlined
 * Reduced to 1 essential navigation test
 * Full debate flows are covered by integration tests
 * 
 * @desktop - Tests for desktop viewport
 */

test.describe('Debate Flow - Navigation', () => {
  test('should load home page and navigate @desktop', async ({ page }) => {
    const setupPage = new DebateSetupPage(page);
    await setupPage.goto();
    
    // Verify we're on the home page
    await expect(page).toHaveURL(/.*localhost:5000\/?$/);
    
    // Verify key elements are present
    await expect(setupPage.rapper1Container).toBeVisible();
    await expect(setupPage.beginDebateButton).toBeVisible();
  });
});
