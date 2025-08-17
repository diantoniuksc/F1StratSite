function rateLimitPageLoads(limit = 20, windowMs = 60000, key = 'strategygp_reload_timestamps') {
    function getReloads() {
        const raw = localStorage.getItem(key);
        if (!raw) return [];
        try {
            return JSON.parse(raw);
        } catch {
            return [];
        }
    }
    function setReloads(arr) {
        localStorage.setItem(key, JSON.stringify(arr));
    }
    const now = Date.now();
    let reloads = getReloads().filter(ts => now - ts < windowMs);
    reloads.push(now);
    setReloads(reloads);
    if (reloads.length > limit) {
        var currentUrl = encodeURIComponent(window.location.pathname + window.location.search);
        window.location.href = '/RateExceeded?returnUrl=' + currentUrl;
        throw new Error('Rate limit exceeded');
    }
}
