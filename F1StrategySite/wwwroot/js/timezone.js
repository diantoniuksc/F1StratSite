// Set an element's text to the user's local time for a given ISO UTC string
// Usage: setLocalTime('elementId', '2025-09-14T13:00:00Z');
function setLocalTime(elementId, isoUtcString) {
  try {
    const el = document.getElementById(elementId);
    if (!el || !isoUtcString) return;
    const date = new Date(isoUtcString); // interpreted as UTC when 'Z' present
  // Format explicitly as dd-MM-yyyy HH:mm:ss in the user's local timezone
  const pad2 = (n) => String(n).padStart(2, '0');
  const day = pad2(date.getDate());
  const month = pad2(date.getMonth() + 1);
  const year = date.getFullYear();
  const hours = pad2(date.getHours());
  const minutes = pad2(date.getMinutes());
  const seconds = pad2(date.getSeconds());
  el.textContent = `${day}-${month}-${year} ${hours}:${minutes}:${seconds}`;
  } catch (_) { /* noop */ }
}

// Expose globally
window.setLocalTime = setLocalTime;
