import { spawn, ChildProcess } from 'child_process';
import * as path from 'path';

let azuriteProcess: ChildProcess | null = null;

/**
 * Global setup for Playwright E2E tests
 * Starts Azurite storage emulator before the web server starts
 */
async function globalSetup() {
  console.log('🚀 Starting Azurite storage emulator...');
  
  const projectRoot = path.resolve(__dirname, '../..');
  
  // Start Azurite as a background process
  azuriteProcess = spawn('npx', [
    'azurite',
    '--silent',
    '--location', projectRoot,
    '--blobHost', '127.0.0.1',
    '--queueHost', '127.0.0.1', 
    '--tableHost', '127.0.0.1'
  ], {
    cwd: projectRoot,
    shell: true,
    detached: false,
    stdio: ['ignore', 'pipe', 'pipe']
  });

  // Store the process ID for teardown
  if (azuriteProcess.pid) {
    process.env.AZURITE_PID = azuriteProcess.pid.toString();
    console.log(`✅ Azurite started with PID: ${azuriteProcess.pid}`);
  }

  // Log Azurite output for debugging
  azuriteProcess.stdout?.on('data', (data) => {
    const msg = data.toString().trim();
    if (msg) console.log(`[Azurite] ${msg}`);
  });

  azuriteProcess.stderr?.on('data', (data) => {
    const msg = data.toString().trim();
    if (msg) console.error(`[Azurite Error] ${msg}`);
  });

  azuriteProcess.on('error', (err) => {
    console.error('Failed to start Azurite:', err);
  });

  // Wait for Azurite to be ready (Table service on port 10002)
  await waitForAzurite();
  console.log('✅ Azurite is ready!');
}

async function waitForAzurite(maxAttempts = 30, delayMs = 1000): Promise<void> {
  const net = await import('net');
  
  for (let attempt = 1; attempt <= maxAttempts; attempt++) {
    try {
      await new Promise<void>((resolve, reject) => {
        const socket = new net.Socket();
        socket.setTimeout(1000);
        
        socket.on('connect', () => {
          socket.destroy();
          resolve();
        });
        
        socket.on('error', () => {
          socket.destroy();
          reject(new Error('Connection failed'));
        });
        
        socket.on('timeout', () => {
          socket.destroy();
          reject(new Error('Connection timeout'));
        });
        
        socket.connect(10002, '127.0.0.1');
      });
      
      return; // Success!
    } catch {
      if (attempt === maxAttempts) {
        throw new Error(`Azurite did not start after ${maxAttempts} attempts`);
      }
      console.log(`⏳ Waiting for Azurite... (attempt ${attempt}/${maxAttempts})`);
      await new Promise(r => setTimeout(r, delayMs));
    }
  }
}

export default globalSetup;
