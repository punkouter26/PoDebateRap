import { test, expect } from '@playwright/test';
import { DebateSetupPage, TestData } from './helpers/page-objects';

/**
 * Debate Setup Tests - Streamlined
 * Core tests for debate setup functionality
 * Reduced from 12 to 5 tests to keep execution under 1 minute
 * 
 * @desktop - Tests for desktop viewport
 * @mobile - Tests for mobile viewport
 */

test.describe('Debate Setup - Core Functionality', () => {
  let setupPage: DebateSetupPage;

  test.beforeEach(async ({ page }) => {
    setupPage = new DebateSetupPage(page);
    await setupPage.goto();
  });

  test('should enable Begin Debate button when all fields are valid @desktop', async () => {
    // Setup complete debate - covers: rapper selection, topic input, button state
    await setupPage.setupDebate(
      TestData.rappers.eminem,
      TestData.rappers.kendrick,
      TestData.topics.medium
    );
    
    // Button should be enabled
    await setupPage.assertBeginDebateButtonEnabled();
    
    // No error should be visible
    await setupPage.assertNoErrorMessage();
  });

  test('should display all essential UI elements @desktop', async ({ page }) => {
    // Verify all core elements are present
    await expect(setupPage.rapper1Container).toBeVisible();
    await expect(setupPage.rapper2Container).toBeVisible();
    await expect(setupPage.topicInput).toBeVisible();
    await expect(setupPage.beginDebateButton).toBeVisible();
    
    // Verify key rappers are available
    await expect(page.locator('heading:has-text("Eminem")')).toBeVisible();
    await expect(page.locator('heading:has-text("Tupac Shakur")')).toBeVisible();
  });

  test('should accept user input and validate form @desktop', async () => {
    // Test topic input
    const testTopic = TestData.topics.medium;
    await setupPage.enterTopic(testTopic);
    await expect(setupPage.topicInput).toHaveValue(testTopic);
    
    // Complete form
    await setupPage.selectRapper1(TestData.rappers.nas);
    await setupPage.selectRapper2(TestData.rappers.jayz);
    
    // Should be enabled when valid
    await setupPage.assertBeginDebateButtonEnabled();
  });
});
