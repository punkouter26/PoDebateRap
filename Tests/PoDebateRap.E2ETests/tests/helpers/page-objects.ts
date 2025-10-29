import { Page, expect } from '@playwright/test';

/**
 * Page Object Model for the Debate Setup page
 */
export class DebateSetupPage {
  constructor(private page: Page) {}

  // Locators
  get rapper1Container() {
    return this.page.locator('text=Select Rapper 1').locator('..').locator('..');
  }

  get rapper2Container() {
    return this.page.locator('text=Select Rapper 2').locator('..').locator('..');
  }

  get topicInput() {
    return this.page.locator('input[placeholder="Enter debate topic..."]');
  }

  get beginDebateButton() {
    return this.page.locator('button:has-text("Begin Debate")');
  }

  get errorMessage() {
    return this.page.locator('.error-message, .alert-danger');
  }

  get loadingIndicator() {
    return this.page.locator('.loading, .spinner');
  }

  get quickBattleButton() {
    return this.page.locator('button:has-text("Quick Battle")');
  }

  // Actions
  async goto() {
    await this.page.goto('/');
    await this.page.waitForLoadState('networkidle');
    // Wait for Blazor to initialize
    await this.page.waitForTimeout(2000);
  }

  async selectRapper1(rapperName: string) {
    // The rapper selection uses card-based UI. The cards have class="rapper-card"
    // and contain the rapper name. Click on the card, not the heading.
    const rapperCard = this.rapper1Container.locator(`.rapper-card:has-text("${rapperName}")`).first();
    await rapperCard.scrollIntoViewIfNeeded();  // Ensure visible on mobile
    await rapperCard.click({ timeout: 10000 });
    await this.page.waitForTimeout(500);
  }

  async selectRapper2(rapperName: string) {
    // The rapper selection uses card-based UI. The cards have class="rapper-card"
    // and contain the rapper name. Click on the card, not the heading.
    const rapperCard = this.rapper2Container.locator(`.rapper-card:has-text("${rapperName}")`).first();
    await rapperCard.scrollIntoViewIfNeeded();  // Ensure visible on mobile
    await rapperCard.click({ timeout: 10000 });
    await this.page.waitForTimeout(500);
  }

  async enterTopic(topic: string) {
    await this.topicInput.scrollIntoViewIfNeeded();  // Ensure visible on mobile
    await this.topicInput.fill(topic);
    await this.page.waitForTimeout(500);
  }

  async clickBeginDebate() {
    await this.beginDebateButton.scrollIntoViewIfNeeded();  // Ensure visible on mobile
    await this.beginDebateButton.click();
  }

  async setupDebate(rapper1: string, rapper2: string, topic: string) {
    await this.selectRapper1(rapper1);
    await this.selectRapper2(rapper2);
    await this.enterTopic(topic);
  }

  // Assertions
  async assertBeginDebateButtonEnabled() {
    await expect(this.beginDebateButton).toBeEnabled();
  }

  async assertBeginDebateButtonDisabled() {
    await expect(this.beginDebateButton).toBeDisabled();
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
