import { test, expect } from '@playwright/test';
import { DiagnosticsPage } from './helpers/page-objects';

/**
 * Diagnostics Tests - Streamlined
 * Reduced from 8 to 3 essential tests
 * 
 * @desktop - Tests for desktop viewport
 */

test.describe('Diagnostics - Core Functionality', () => {
  test('should load diagnostics page and display health statuses @desktop', async ({ page }) => {
    const diagPage = new DiagnosticsPage(page);
    await diagPage.goto();
    
    // Page should load successfully
    await expect(page).toHaveURL(/.*diag.*/);
    
    // All key status elements should be visible
    await expect(diagPage.openAiStatus).toBeVisible();
    await expect(diagPage.speechStatus).toBeVisible();
    await expect(diagPage.storageStatus).toBeVisible();
    await expect(diagPage.newsStatus).toBeVisible();
  });

  test('should navigate directly via URL @desktop', async ({ page }) => {
    // Direct navigation to /diag
    await page.goto('/diag');
    await page.waitForLoadState('networkidle');
    
    // Should be on diagnostics page
    await expect(page).toHaveURL(/.*diag.*/);
  });
});
