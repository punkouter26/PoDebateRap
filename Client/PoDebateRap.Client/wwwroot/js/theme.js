// Theme management functions

window.getTheme = () => {
    // Check localStorage first
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        console.log('📦 Loaded saved theme:', savedTheme);
        return savedTheme;
    }
    
    // Otherwise check system preference
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const systemTheme = prefersDark ? 'dark' : 'light';
    console.log('🎨 Using system theme preference:', systemTheme);
    return systemTheme;
};

window.saveTheme = (theme) => {
    localStorage.setItem('theme', theme);
    console.log('💾 Saved theme:', theme);
};

window.applyTheme = (theme) => {
    document.documentElement.setAttribute('data-theme', theme);
    console.log('✨ Applied theme:', theme);
    
    // Add smooth transition class
    document.documentElement.classList.add('theme-transitioning');
    setTimeout(() => {
        document.documentElement.classList.remove('theme-transitioning');
    }, 300);
};

// Auto-detect system theme changes
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
    const newTheme = e.matches ? 'dark' : 'light';
    const savedTheme = localStorage.getItem('theme');
    
    // Only auto-switch if user hasn't manually set a preference
    if (!savedTheme) {
        console.log('🔄 System theme changed to:', newTheme);
        applyTheme(newTheme);
    }
});

// Apply initial theme on page load
document.addEventListener('DOMContentLoaded', () => {
    const initialTheme = window.getTheme();
    window.applyTheme(initialTheme);
});
