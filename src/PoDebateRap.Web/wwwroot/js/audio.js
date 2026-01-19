// Audio playback functionality for PoDebateRap
window.playAudio = function (dotNetRef, base64Audio) {
    return new Promise((resolve, reject) => {
        try {
            // Create audio context if not exists
            if (!window.audioContext) {
                window.audioContext = new (window.AudioContext || window.webkitAudioContext)();
            }

            // Decode base64 to array buffer
            const binaryString = atob(base64Audio);
            const len = binaryString.length;
            const bytes = new Uint8Array(len);
            for (let i = 0; i < len; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }

            // Create a blob and URL for the audio
            const blob = new Blob([bytes], { type: 'audio/wav' });
            const url = URL.createObjectURL(blob);

            // Create and play audio element
            const audio = new Audio(url);
            
            audio.onended = function () {
                URL.revokeObjectURL(url);
                // Notify .NET that playback completed
                if (dotNetRef && dotNetRef.invokeMethodAsync) {
                    dotNetRef.invokeMethodAsync('NotifyAudioPlaybackComplete');
                }
                resolve();
            };

            audio.onerror = function (e) {
                console.error('Audio playback error:', e);
                URL.revokeObjectURL(url);
                // Still notify completion so the debate can continue
                if (dotNetRef && dotNetRef.invokeMethodAsync) {
                    dotNetRef.invokeMethodAsync('NotifyAudioPlaybackComplete');
                }
                reject(e);
            };

            // Resume audio context if suspended (browser autoplay policy)
            if (window.audioContext.state === 'suspended') {
                window.audioContext.resume();
            }

            audio.play().catch(function (error) {
                console.error('Audio play failed:', error);
                // Notify completion even on failure
                if (dotNetRef && dotNetRef.invokeMethodAsync) {
                    dotNetRef.invokeMethodAsync('NotifyAudioPlaybackComplete');
                }
                reject(error);
            });

        } catch (error) {
            console.error('Error in playAudio:', error);
            // Notify completion on error
            if (dotNetRef && dotNetRef.invokeMethodAsync) {
                dotNetRef.invokeMethodAsync('NotifyAudioPlaybackComplete');
            }
            reject(error);
        }
    });
};

// Stop any currently playing audio
window.stopAudio = function () {
    if (window.currentAudio) {
        window.currentAudio.pause();
        window.currentAudio = null;
    }
};
