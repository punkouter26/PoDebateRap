// Touch Gesture Handler
// Detects swipe gestures and provides callbacks for different directions

window.gestureHandler = {
    touchStartX: 0,
    touchStartY: 0,
    touchEndX: 0,
    touchEndY: 0,
    minSwipeDistance: 50, // Minimum distance for a swipe to be recognized
    maxVerticalDeviation: 100, // Max vertical movement for horizontal swipes
    
    callbacks: {
        swipeLeft: null,
        swipeRight: null,
        swipeDown: null,
        swipeUp: null
    },

    // Initialize gesture detection on an element
    init: (elementSelector, callbacks) => {
        const element = document.querySelector(elementSelector);
        if (!element) {
            console.warn('Gesture element not found:', elementSelector);
            return;
        }

        // Store callbacks
        window.gestureHandler.callbacks = callbacks || {};

        // Add touch event listeners
        element.addEventListener('touchstart', window.gestureHandler.handleTouchStart, { passive: true });
        element.addEventListener('touchend', window.gestureHandler.handleTouchEnd, { passive: true });

        console.log('✋ Gesture handler initialized on:', elementSelector);
    },

    handleTouchStart: (event) => {
        window.gestureHandler.touchStartX = event.changedTouches[0].screenX;
        window.gestureHandler.touchStartY = event.changedTouches[0].screenY;
    },

    handleTouchEnd: (event) => {
        window.gestureHandler.touchEndX = event.changedTouches[0].screenX;
        window.gestureHandler.touchEndY = event.changedTouches[0].screenY;
        window.gestureHandler.handleGesture();
    },

    handleGesture: () => {
        const diffX = window.gestureHandler.touchEndX - window.gestureHandler.touchStartX;
        const diffY = window.gestureHandler.touchEndY - window.gestureHandler.touchStartY;
        const absDiffX = Math.abs(diffX);
        const absDiffY = Math.abs(diffY);

        // Horizontal swipe detection
        if (absDiffX > window.gestureHandler.minSwipeDistance && 
            absDiffY < window.gestureHandler.maxVerticalDeviation) {
            
            if (diffX > 0) {
                // Swipe right
                console.log('👉 Swipe right detected');
                if (window.gestureHandler.callbacks.swipeRight) {
                    window.gestureHandler.callbacks.swipeRight();
                }
            } else {
                // Swipe left
                console.log('👈 Swipe left detected');
                if (window.gestureHandler.callbacks.swipeLeft) {
                    window.gestureHandler.callbacks.swipeLeft();
                }
            }
        }
        // Vertical swipe detection
        else if (absDiffY > window.gestureHandler.minSwipeDistance && 
                 absDiffX < window.gestureHandler.maxVerticalDeviation) {
            
            if (diffY > 0) {
                // Swipe down
                console.log('👇 Swipe down detected');
                if (window.gestureHandler.callbacks.swipeDown) {
                    window.gestureHandler.callbacks.swipeDown();
                }
            } else {
                // Swipe up
                console.log('👆 Swipe up detected');
                if (window.gestureHandler.callbacks.swipeUp) {
                    window.gestureHandler.callbacks.swipeUp();
                }
            }
        }
    },

    // Clean up event listeners
    destroy: (elementSelector) => {
        const element = document.querySelector(elementSelector);
        if (element) {
            element.removeEventListener('touchstart', window.gestureHandler.handleTouchStart);
            element.removeEventListener('touchend', window.gestureHandler.handleTouchEnd);
            console.log('🗑️ Gesture handler destroyed');
        }
    }
};

// Helper function for .NET interop
window.initializeGestures = (dotnetHelper, elementSelector) => {
    window.gestureHandler.init(elementSelector, {
        swipeLeft: () => {
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('OnSwipeLeft')
                    .catch(e => console.error('Error calling OnSwipeLeft:', e));
            }
        },
        swipeRight: () => {
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('OnSwipeRight')
                    .catch(e => console.error('Error calling OnSwipeRight:', e));
            }
        },
        swipeDown: () => {
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('OnSwipeDown')
                    .catch(e => console.error('Error calling OnSwipeDown:', e));
            }
        },
        swipeUp: () => {
            if (dotnetHelper) {
                dotnetHelper.invokeMethodAsync('OnSwipeUp')
                    .catch(e => console.error('Error calling OnSwipeUp:', e));
            }
        }
    });
};

window.destroyGestures = (elementSelector) => {
    window.gestureHandler.destroy(elementSelector);
};
