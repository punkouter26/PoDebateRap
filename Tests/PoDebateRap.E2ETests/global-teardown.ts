/**
 * Global teardown for Playwright E2E tests
 * Stops Azurite storage emulator after all tests complete
 */
async function globalTeardown() {
  const azuritePid = process.env.AZURITE_PID;
  
  if (azuritePid) {
    console.log(`🛑 Stopping Azurite (PID: ${azuritePid})...`);
    try {
      // On Windows, we need to kill the process tree
      if (process.platform === 'win32') {
        const { exec } = await import('child_process');
        await new Promise<void>((resolve, reject) => {
          exec(`taskkill /pid ${azuritePid} /T /F`, (error) => {
            if (error) {
              // Process might have already exited
              console.log('Azurite process may have already exited');
            }
            resolve();
          });
        });
      } else {
        process.kill(parseInt(azuritePid), 'SIGTERM');
      }
      console.log('✅ Azurite stopped');
    } catch (error) {
      console.log('Note: Could not stop Azurite - it may have already exited');
    }
  }
}

export default globalTeardown;
