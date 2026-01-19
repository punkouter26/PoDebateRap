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

  test('should allow topic entry in quick battle mode @desktop', async () => {
    // Enter topic - rappers are auto-selected in quick battle
    await setupPage.enterTopic(TestData.topics.medium);
    
    // Topic should be entered
    await expect(setupPage.topicInput).toHaveValue(TestData.topics.medium);
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
    
    // Quick Battle button may be disabled until rappers load
    await expect(setupPage.startBattleButton).toBeVisible();
  });
});
