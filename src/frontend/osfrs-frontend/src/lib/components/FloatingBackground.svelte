<script lang="ts">
  import { onMount, onDestroy } from "svelte";
  import { SvelteMap } from "svelte/reactivity";

  type Icon = {
    id: string;
    baseX: number;
    baseY: number;
    rotation: number;
    speed: number;
    phase: number;
    scale: number;
    opacity: number;
    src: string;
  };

  const ICONS = [
    "/icon1.svg",
    "/icon2.svg",
    "/icon3.svg",
    "/icon4.svg",
    "/icon5.svg",
  ];

  const GRID_OFFSET_X = 0.25;
  const GRID_OFFSET_Y = 0.25;

  let root: HTMLDivElement | null = null;
  let icons: Icon[] = [];
  let iconEls: HTMLImageElement[] = [];

  let raf: number | null = null;
  let resizeTimeout: number | null = null;
  let lastWidth = 0;

  let generationOpacity = 1;

  let scrolling = false;
  let scrollTimeout: number | null = null;

  let lastFrameTime = 0;
  const MOBILE_FRAME_INTERVAL = 32; // ~30fps

  const rand = (min: number, max: number) => min + Math.random() * (max - min);

  function applyInitialTransforms() {
    for (let i = 0; i < icons.length; i++) {
      const icon = icons[i];
      const el = iconEls[i];
      if (!el) continue;

      el.style.transform = `translate3d(${icon.baseX}px, ${icon.baseY}px, 0)
       rotate(${icon.rotation}deg)
       scale(${icon.scale})`;
    }
  }

  function getColumnCount(vw: number) {
    if (vw < 480) return 2;
    if (vw < 900) return 3;
    if (vw < 1440) return 4;
    return 5;
  }

  function pickIcon(row: number, col: number, placed: Map<string, string>) {
    const left = placed.get(`${row}x${col - 1}`);
    const top = placed.get(`${row - 1}x${col}`);
    const forbidden = new Set([left, top]);
    const choices = ICONS.filter((i) => !forbidden.has(i));
    return choices.length
      ? choices[Math.floor(Math.random() * choices.length)]
      : ICONS[Math.floor(Math.random() * ICONS.length)];
  }

  function generateIcons() {
    const vw = window.innerWidth;
    const docH = document.documentElement.scrollHeight;

    if (root) root.style.height = `${docH}px`;

    const cols = getColumnCount(vw);
    const approxRowHeight = 320;
    const rows = Math.max(1, Math.floor(docH / approxRowHeight));

    const cellW = vw / cols;
    const cellH = docH / rows;

    const jitterX = Math.min(cellW * 0.25, 60);
    const jitterY = Math.min(cellH * 0.25, 60);

    const placed = new SvelteMap<string, string>();
    const result: Icon[] = [];

    for (let row = 0; row < rows; row++) {
      for (let col = 0; col < cols; col++) {
        const baseX =
          col * cellW + cellW * GRID_OFFSET_X + rand(-jitterX, jitterX);

        const baseY =
          row * cellH + cellH * GRID_OFFSET_Y + rand(-jitterY, jitterY);

        const src = pickIcon(row, col, placed);
        placed.set(`${row}x${col}`, src);

        result.push({
          id: `${row}x${col}`,
          baseX,
          baseY,
          rotation: rand(-12, 12),
          speed: rand(0.0004, 0.001),
          phase: Math.random() * Math.PI * 2,
          scale: rand(0.6, 1.0),
          opacity: rand(0.18, 0.38),
          src,
        });
      }
    }

    icons = result;
    iconEls = [];
  }

  function animate(time: number) {
    // throttle on mobile
    if (time - lastFrameTime < MOBILE_FRAME_INTERVAL) {
      raf = requestAnimationFrame(animate);
      return;
    }
    lastFrameTime = time;

    if (!scrolling) {
      for (let i = 0; i < icons.length; i++) {
        const icon = icons[i];
        const el = iconEls[i];
        if (!el) continue;

        const x = icon.baseX + Math.sin(time * icon.speed + icon.phase) * 12;
        const y = icon.baseY + Math.cos(time * icon.speed + icon.phase) * 10;

        el.style.transform = `translate3d(${x}px, ${y}px, 0)
           rotate(${icon.rotation}deg)
           scale(${icon.scale})`;
      }
    }

    raf = requestAnimationFrame(animate);
  }

  function onResize() {
    const w = window.innerWidth;
    if (Math.abs(w - lastWidth) < 20) return;
    lastWidth = w;

    if (resizeTimeout) clearTimeout(resizeTimeout);

    generationOpacity = 0;

    resizeTimeout = window.setTimeout(() => {
      if (raf) {
        cancelAnimationFrame(raf);
        raf = null;
      }

      icons = [];
      iconEls = [];

      requestAnimationFrame(() => {
        generateIcons();

        requestAnimationFrame(() => {
          applyInitialTransforms(); // ← CRITICAL
          generationOpacity = 1;
          raf = requestAnimationFrame(animate);
        });
      });

      resizeTimeout = null;
    }, 180);
  }

  function onScroll() {
    scrolling = true;
    if (scrollTimeout) clearTimeout(scrollTimeout);
    scrollTimeout = window.setTimeout(() => {
      scrolling = false;
    }, 120);
  }

  onMount(() => {
    generateIcons();
    raf = requestAnimationFrame(animate);

    window.addEventListener("resize", onResize, { passive: true });
    window.addEventListener("scroll", onScroll, { passive: true });

    return () => {
      window.removeEventListener("resize", onResize);
      window.removeEventListener("scroll", onScroll);
      if (raf) cancelAnimationFrame(raf);
    };
  });

  onDestroy(() => {
    if (raf) cancelAnimationFrame(raf);
  });
</script>

<div class="bg-root" bind:this={root} style="opacity: {generationOpacity}">
  {#each icons as icon, i (icon.id)}
    <img
      bind:this={iconEls[i]}
      src={icon.src}
      alt=""
      class="bg-icon"
      style="opacity: {icon.opacity}"
      draggable="false"
    />
  {/each}
</div>

<style>
  .bg-root {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    z-index: 0;
    transition: opacity 220ms ease;
  }

  .bg-icon {
    position: absolute;
    width: 96px;
    height: 96px;
    opacity: 0.35;
    mix-blend-mode: soft-light;
    transform-origin: center;
    user-select: none;
    will-change: transform;
  }

  @media (min-width: 1024px) {
    .bg-icon {
      width: 128px;
      height: 128px;
    }
  }

  /* filters only on tablet+ */
  @media (min-width: 768px) {
    .bg-icon {
      filter: blur(4px) saturate(0.75) hue-rotate(-8deg);
    }
  }
</style>
