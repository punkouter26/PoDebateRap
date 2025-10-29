// Haptic Feedback utility
// Uses the Vibration API for tactile feedback on mobile devices

window.haptic = {
    // Check if vibration API is supported
    isSupported: () => {
        return 'vibrate' in navigator;
    },

    // Light tap (e.g., button press, selection)
    light: () => {
        if (window.haptic.isSupported()) {
            navigator.vibrate(10);
            console.log('📳 Haptic: light tap');
        }
    },

    // Medium pulse (e.g., action confirmation)
    medium: () => {
        if (window.haptic.isSupported()) {
            navigator.vibrate(30);
            console.log('📳 Haptic: medium pulse');
        }
    },

    // Heavy pulse (e.g., important action)
    heavy: () => {
        if (window.haptic.isSupported()) {
            navigator.vibrate(50);
            console.log('📳 Haptic: heavy pulse');
        }
    },

    // Success pattern (e.g., debate started, winner announced)
    success: () => {
        if (window.haptic.isSupported()) {
            navigator.vibrate([30, 50, 30]);
            console.log('📳 Haptic: success pattern');
        }
    },

    // Notification pattern (e.g., turn transition)
    notification: () => {
        if (window.haptic.isSupported()) {
            navigator.vibrate([10, 30, 10]);
            console.log('📳 Haptic: notification pattern');
        }
    },

    // Victory celebration (e.g., winner announcement)
    victory: () => {
        if (window.haptic.isSupported()) {
            navigator.vibrate([50, 100, 50, 100, 80]);
            console.log('📳 Haptic: victory celebration');
        }
    },

    // Error pattern
    error: () => {
        if (window.haptic.isSupported()) {
            navigator.vibrate([100, 50, 100]);
            console.log('📳 Haptic: error pattern');
        }
    }
};

// Log vibration API support status
if (window.haptic.isSupported()) {
    console.log('✅ Vibration API is supported');
} else {
    console.log('❌ Vibration API is not supported on this device/browser');
}
