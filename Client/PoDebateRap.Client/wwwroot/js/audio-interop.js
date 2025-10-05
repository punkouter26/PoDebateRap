window.currentAudio = null; // Global variable to hold the current audio object

window.playAudio = (dotnetHelper, base64String) => { // Accept dotnetHelper
    console.log("🎵 playAudio called with base64 length:", base64String ? base64String.length : 0);
    console.log("🎵 Base64 prefix (first 50 chars):", base64String ? base64String.substring(0, 50) : "null");
    console.log("🎵 dotnetHelper:", dotnetHelper ? "Present" : "Missing");
    
    try {
        // Stop any currently playing audio first
        if (window.currentAudio) {
            window.currentAudio.pause();
            window.currentAudio.currentTime = 0;
            console.log("🛑 Stopped previous audio.");
        }
        
        // Use only WAV format which matches what the server returns
        const audioDataUrl = `data:audio/wav;base64,${base64String}`;
        
        console.log("🎵 Creating audio with WAV format, data URL length:", audioDataUrl.length);
        try {
            window.currentAudio = new Audio(audioDataUrl);
            console.log("✅ Audio object created successfully");
        } catch (error) {
            console.error("❌ Failed to create audio object:", error);
            throw error;
        }
        
        // Add debugging to check if the audio is valid
        if (window.currentAudio.error) {
            console.error("❌ Audio has error immediately after creation:", window.currentAudio.error);
            throw new Error("Audio object created with error");
        }

        // Set volume to ensure it's audible
        window.currentAudio.volume = 1.0; // Increase to max volume
        console.log("🔊 Volume set to:", window.currentAudio.volume);

        window.currentAudio.addEventListener('loadstart', () => {
            console.log("📥 Audio loading started");
        });

        window.currentAudio.addEventListener('canplay', () => {
            console.log("✅ Audio can start playing - ready state:", window.currentAudio.readyState);
        });

        window.currentAudio.addEventListener('playing', () => {
            console.log("▶️ Audio is now PLAYING!");
        });

        window.currentAudio.addEventListener('ended', () => {
            console.log("🏁 Audio playback finished.");
            window.currentAudio = null; // Clear reference on end
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('NotifyAudioPlaybackComplete')
                    .catch(e => console.error("Error invoking .NET method:", e));
            }
        });

        window.currentAudio.addEventListener('error', (e) => {
            console.error("❌ Error during audio playback:", e);
            console.error("❌ Audio error details:", window.currentAudio.error);
            if (window.currentAudio.error) {
                console.error("❌ Error code:", window.currentAudio.error.code);
                console.error("❌ Error message:", window.currentAudio.error.message);
            }
            window.currentAudio = null; // Clear reference on error
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('NotifyAudioPlaybackComplete')
                    .catch(e => console.error("Error invoking .NET method after playback error:", e));
            }
        });

        // Attempt to play with user interaction handling
        console.log("🎬 Attempting to play audio...");
        const playPromise = window.currentAudio.play();
        
        if (playPromise !== undefined) {
            playPromise
                .then(() => {
                    console.log("✅ Audio playback started successfully");
                })
                .catch(e => {
                    console.error("❌ Error starting audio playback:", e);
                    console.error("❌ Error name:", e.name, "Error message:", e.message);
                    
                    // If autoplay failed, try to enable audio on next user interaction
                    if (e.name === 'NotAllowedError') {
                        console.warn("Autoplay prevented by browser. Audio will play on next user interaction.");
                        
                        // Create a one-time click handler to enable audio
                        const enableAudio = () => {
                            console.log("User interaction detected, attempting to play audio");
                            window.currentAudio.play()
                                .then(() => console.log("Audio started after user interaction"))
                                .catch(err => console.error("Still failed to play audio:", err));
                            document.removeEventListener('click', enableAudio);
                        };
                        
                        document.addEventListener('click', enableAudio);
                    }
                    
                    window.currentAudio = null; // Clear reference on start error
                    if (dotnetHelper) {
                        dotnetHelper.invokeMethodAsync('NotifyAudioPlaybackComplete')
                            .catch(e => console.error("Error invoking .NET method after playback start error:", e));
                    }
                });
        }
        
        console.log("Audio setup completed");
    } catch (e) {
        console.error("Error creating or playing audio:", e);
        window.currentAudio = null; // Clear reference on creation error
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('NotifyAudioPlaybackComplete')
                .catch(e => console.error("Error invoking .NET method after audio creation error:", e));
        }
    }
};

window.stopAudio = () => {
    if (window.currentAudio) {
        window.currentAudio.pause();
        window.currentAudio.currentTime = 0;
        window.currentAudio = null; // Clear the reference
        console.log("Audio stopped via JS call.");
        return true; // Indicate audio was stopped
    }
    console.log("No audio playing to stop.");
    return false; // Indicate no audio was playing
};    // Test function to verify audio capability
window.testAudio = () => {
    console.log("Testing audio capability...");
    const testAudio = new Audio();
    console.log("Audio object created:", testAudio);
    console.log("Can play WAV:", testAudio.canPlayType('audio/wav'));
    console.log("Can play MP3:", testAudio.canPlayType('audio/mpeg'));
    console.log("Can play OGG:", testAudio.canPlayType('audio/ogg'));
};

// Function to inspect the base64 data to help diagnose issues
window.inspectAudioData = (base64String) => {
    if (!base64String || base64String.length === 0) {
        console.error("Empty base64 string provided");
        return;
    }
    
    // Log the first 50 characters
    console.log("Base64 data starts with:", base64String.substring(0, 50));
    
    // Try to decode a small sample of the base64 to check if it's valid
    try {
        const sampleBytes = atob(base64String.substring(0, 100));
        console.log("First bytes decoded successfully, length:", sampleBytes.length);
        
        // Check for RIFF header (common in WAV files)
        const header = sampleBytes.substring(0, 4);
        console.log("First 4 chars:", Array.from(header).map(c => c.charCodeAt(0)));
        
        if (header === 'RIFF') {
            console.log("Valid RIFF header detected");
        } else {
            console.warn("No RIFF header found, might not be a standard WAV file");
        }
    } catch (e) {
        console.error("Error decoding base64:", e);
    }
};

// Call test on page load
document.addEventListener('DOMContentLoaded', () => {
    window.testAudio();
});
