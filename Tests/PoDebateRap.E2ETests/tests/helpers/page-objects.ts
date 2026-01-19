import { Page, expect } from '@playwright/test';

/**
 * Page Object Model for the Debate Setup page (Quick Battle UI)
 * Updated for simplified UI - topic input + quick battle button only
 */
export class DebateSetupPage {
  constructor(private page: Page) {}

  // Locators - Updated for new Quick Battle UI
  get topicLabel() {
    return this.page.locator('text=What should they debate?');
  }

  get topicHint() {
    return this.page.locator('text=Enter a topic and two random rappers will argue Pro vs Con');
  }

  get topicInput() {
    return this.page.locator('input.topic-input, .topic-input input, input[placeholder*="topic" i]').first();
  }

  get startBattleButton() {
    return this.page.locator('button:has-text("START"), button:has-text("Quick Battle"), .start-debate-btn, .quick-battle-btn').first();
  }

  get stopDebateButton() {
    return this.page.locator('button:has-text("STOP"), .stop-debate-btn').first();
  }

  get quickBattleHint() {
    return this.page.locator('text=Two random legendary rappers will face off');
  }

  get loadingIndicator() {
    return this.page.locator('.loading, .spinner');
  }

  get mainTitle() {
    return this.page.locator('h1:has-text("PoDebateRap")');
  }

  get subtitle() {
    return this.page.locator('text=AI-Powered Rap Battle Arena');
  }

  get emptyStateMessage() {
    return this.page.locator('text=Ready to Battle?');
  }

  // Legacy locators - kept for backwards compatibility (these won't find elements in new UI)
  get rapper1Container() {
    return this.page.locator('[data-testid="rapper1-container"]');
  }

  get rapper2Container() {
    return this.page.locator('[data-testid="rapper2-container"]');
  }

  get beginDebateButton() {
    return this.startBattleButton; // Alias for backwards compatibility
  }

  get errorMessage() {
    return this.page.locator('.error-message, .alert-danger, .alert-error');
  }

  get quickBattleButton() {
    return this.startBattleButton; // Same button now
  }

  // Actions
  async goto() {
    await this.page.goto('/');
    await this.page.waitForLoadState('networkidle');
    // Wait for Blazor to initialize
    await this.page.waitForTimeout(2000);
  }

  async enterTopic(topic: string) {
    await this.topicInput.scrollIntoViewIfNeeded();
    await this.topicInput.fill(topic);
    await this.page.waitForTimeout(500);
  }

  async clickStartBattle() {
    await this.startBattleButton.scrollIntoViewIfNeeded();
    await this.startBattleButton.click();
  }

  async clickStopDebate() {
    await this.stopDebateButton.scrollIntoViewIfNeeded();
    await this.stopDebateButton.click();
  }

  // Legacy methods - simplified for quick battle (rappers are auto-selected)
  async selectRapper1(rapperName: string) {
    console.log(`selectRapper1 called with ${rapperName} - skipped (auto-selection in quick battle)`);
  }

  async selectRapper2(rapperName: string) {
    console.log(`selectRapper2 called with ${rapperName} - skipped (auto-selection in quick battle)`);
  }

  async clickBeginDebate() {
    await this.clickStartBattle();
  }

  async setupDebate(rapper1: string, rapper2: string, topic: string) {
    // Simplified - just enter topic, rappers are auto-selected
    await this.enterTopic(topic);
  }

  // Assertions
  async assertStartBattleButtonEnabled() {
    await expect(this.startBattleButton).toBeEnabled();
  }

  async assertStartBattleButtonDisabled() {
    await expect(this.startBattleButton).toBeDisabled();
  }

  // Legacy aliases
  async assertBeginDebateButtonEnabled() {
    await this.assertStartBattleButtonEnabled();
  }

  async assertBeginDebateButtonDisabled() {
    await this.assertStartBattleButtonDisabled();
  }

  async assertErrorMessageVisible(message?: string) {
    await expect(this.errorMessage).toBeVisible();
    if (message) {
      await expect(this.errorMessage).toContainText(message);
    }
  }

  async assertNoErrorMessage() {
    await expect(this.errorMessage).not.toBeVisible();
  }
}

/**
 * Page Object Model for the Debate Arena page
 */
export class DebateArenaPage {
  constructor(private page: Page) {}

  // Locators
  get debateArena() {
    return this.page.locator('.debate-arena, [data-testid="debate-arena"]');
  }

  get rapper1Info() {
    return this.page.locator('.rapper1-info, [data-testid="rapper1-info"]');
  }

  get rapper2Info() {
    return this.page.locator('.rapper2-info, [data-testid="rapper2-info"]');
  }

  get verseDisplay() {
    return this.page.locator('.verse-display, [data-testid="verse-display"]');
  }

  get audioPlayer() {
    return this.page.locator('audio');
  }

  get nextTurnButton() {
    return this.page.locator('button:has-text("Next Turn")');
  }

  get endDebateButton() {
    return this.page.locator('button:has-text("End Debate")');
  }

  get turnCounter() {
    return this.page.locator('.turn-counter, [data-testid="turn-counter"]');
  }

  get winnerAnnouncement() {
    return this.page.locator('.winner-announcement, [data-testid="winner"]');
  }

  // Actions
  async waitForDebateStart() {
    await expect(this.debateArena).toBeVisible({ timeout: 30000 });
  }

  async waitForVerse() {
    await expect(this.verseDisplay).toBeVisible({ timeout: 15000 });
  }

  async clickNextTurn() {
    await this.nextTurnButton.click();
  }

  async clickEndDebate() {
    await this.endDebateButton.click();
  }

  async getAudioSrc(): Promise<string | null> {
    return await this.audioPlayer.getAttribute('src');
  }

  async isAudioPlaying(): Promise<boolean> {
    const paused = await this.audioPlayer.evaluate((audio: HTMLAudioElement) => audio.paused);
    return !paused;
  }

  async getAudioVolume(): Promise<number> {
    return await this.audioPlayer.evaluate((audio: HTMLAudioElement) => audio.volume);
  }

  // Assertions
  async assertDebateStarted() {
    await expect(this.debateArena).toBeVisible();
  }

  async assertVerseDisplayed() {
    await expect(this.verseDisplay).toBeVisible();
    await expect(this.verseDisplay).not.toBeEmpty();
  }

  async assertAudioPresent() {
    await expect(this.audioPlayer).toBeVisible();
    const src = await this.getAudioSrc();
    expect(src).toBeTruthy();
  }

  async assertWinnerAnnounced() {
    await expect(this.winnerAnnouncement).toBeVisible({ timeout: 10000 });
  }
}

/**
 * Page Object Model for Diagnostics page
 */
export class DiagnosticsPage {
  constructor(private page: Page) {}

  // Locators
  get healthStatus() {
    return this.page.locator('[data-testid="health-status"]');
  }

  get openAiStatus() {
    return this.page.locator('[data-testid="openai-status"]');
  }

  get speechStatus() {
    return this.page.locator('[data-testid="speech-status"]');
  }

  get storageStatus() {
    return this.page.locator('[data-testid="storage-status"]');
  }

  get newsStatus() {
    return this.page.locator('[data-testid="news-status"]');
  }

  // Actions
  async goto() {
    await this.page.goto('/diag');
    await this.page.waitForLoadState('networkidle');
  }

  // Assertions
  async assertAllServicesHealthy() {
    await expect(this.openAiStatus).toContainText(/healthy|operational/i);
    await expect(this.speechStatus).toContainText(/healthy|operational/i);
    await expect(this.storageStatus).toContainText(/healthy|operational/i);
  }
}

/**
 * Test data helpers
 */
export const TestData = {
  rappers: {
    eminem: 'Eminem',
    tupac: 'Tupac Shakur',
    nas: 'Nas',
    kendrick: 'Kendrick Lamar',
    jayz: 'Jay-Z',
  },
  
  topics: {
    short: 'Test',
    medium: 'Climate Change',
    long: 'The Impact of Artificial Intelligence on Modern Society and Future Generations',
  },
  
  waitTimes: {
    blazorInit: 2000,
    shortWait: 500,
    mediumWait: 1000,
    verseGeneration: 15000,
    audioGeneration: 20000,
  }
};
