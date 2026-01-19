import { test, expect } from '@playwright/test';
import { DebateSetupPage, TestData } from './helpers/page-objects';

/**
 * Debate Flow Tests - Quick Battle
 * Updated for simplified quick battle UI
 * Full debate flows are covered by integration tests
 * 
 * @desktop - Tests for desktop viewport
 */

test.describe('Debate Flow - Quick Battle', () => {
  test('should load home page with quick battle UI @desktop', async ({ page }) => {
    const setupPage = new DebateSetupPage(page);
    await setupPage.goto();
    
    // Verify we're on the home page
    await expect(page).toHaveURL(/.*localhost:7189\/?$/);
    
    // Verify quick battle elements are present
    await expect(setupPage.topicInput).toBeVisible();
    await expect(setupPage.startBattleButton).toBeVisible();
  });

  test('should allow entering topic for battle @desktop', async ({ page }) => {
    const setupPage = new DebateSetupPage(page);
    await setupPage.goto();
    
    // Enter a topic
    await setupPage.enterTopic('AI vs Humans');
    
    // Verify input is filled
    await expect(setupPage.topicInput).toHaveValue('AI vs Humans');
    
    // Quick Battle button should be visible (may be disabled until rappers load)
    await expect(setupPage.startBattleButton).toBeVisible();
  });
});
