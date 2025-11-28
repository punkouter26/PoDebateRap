import { test, expect } from '@playwright/test';
import { DebateSetupPage, TestData } from './helpers/page-objects';

/**
 * Debate Setup Tests - Quick Battle UI
 * Core tests for quick battle functionality
 * Updated for simplified UI with auto-selected rappers
 * 
 * @desktop - Tests for desktop viewport
 * @mobile - Tests for mobile viewport
 */

test.describe('Debate Setup - Quick Battle', () => {
  let setupPage: DebateSetupPage;

  test.beforeEach(async ({ page }) => {
    setupPage = new DebateSetupPage(page);
    await setupPage.goto();
  });

  test('should enable Start Battle button when topic is entered @desktop', async () => {
    // Enter topic - rappers are auto-selected in quick battle
    await setupPage.enterTopic(TestData.topics.medium);
    
    // Button should be enabled
    await setupPage.assertStartBattleButtonEnabled();
    
    // No error should be visible
    await setupPage.assertNoErrorMessage();
  });

  test('should display all essential UI elements @desktop', async ({ page }) => {
    // Verify quick battle UI elements are present
    await expect(setupPage.topicInput).toBeVisible();
    await expect(setupPage.startBattleButton).toBeVisible();
    
    // Verify title/branding
    await expect(page.locator('h1').first()).toBeVisible();
  });

  test('should accept user input in topic field @desktop', async () => {
    // Test topic input
    const testTopic = TestData.topics.medium;
    await setupPage.enterTopic(testTopic);
    await expect(setupPage.topicInput).toHaveValue(testTopic);
    
    // Should be enabled when topic is entered
    await setupPage.assertStartBattleButtonEnabled();
  });
});
