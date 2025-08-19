window.centerAfterBars = {
  init(rootSel, barsSel, imgWrapSel) {
    const root = document.querySelector(rootSel);
    const bars = document.querySelector(barsSel);
    const imgWrap = document.querySelector(imgWrapSel);
    if (!(root && bars && imgWrap)) return;

    function setHeight() {
      const rootH = root.clientHeight;      // total element height
      const barsH = bars.offsetHeight;      // colorful bars height
      const remaining = Math.max(rootH - barsH, 0);
      imgWrap.style.height = remaining + "px";   // center within the leftover
    }

    setHeight();

    // Keep it correct on size/content changes
    const ro = new ResizeObserver(setHeight);
    ro.observe(root);
    ro.observe(bars);
    window.addEventListener("resize", setHeight, { passive: true });
  }
};
