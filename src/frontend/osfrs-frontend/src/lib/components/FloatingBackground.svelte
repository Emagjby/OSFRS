<script lang="ts">
  import { onMount, onDestroy } from "svelte";
  import { SvelteMap } from "svelte/reactivity";

  type Icon = {
    id: string;
    baseX: number;
    baseY: number;
    x: number;
    y: number;
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

  const GRID_OFFSET_X: number = 0.25;
  const GRID_OFFSET_Y: number = 0.25;

  let generationOpacity: number = 1;

  let root: HTMLDivElement | null = null;
  let icons: Icon[] = [];
  let raf: number | null = null;
  let resizeTimeout: number | null = null;
  let lastWidth: number = 0;

  const rand = (min: number, max: number) => min + Math.random() * (max - min);

  function getColumnCount(vw: number) {
    if (vw < 700) return 2;
    if (vw < 1440) return 3;
    if (vw < 1800) return 4;
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
    const vw: number = window.innerWidth;
    const docH: number = document.documentElement.scrollHeight;

    if (root) {
      root.style.height = `${docH}px`;
    }

    const cols: number = getColumnCount(vw);
    const approxRowHeight: number = 320;
    const rows: number = Math.max(1, Math.floor(docH / approxRowHeight));

    const cellW: number = vw / cols;
    const cellH: number = docH / rows;

    const jitterX: number = Math.min(cellW * 0.25, 60);
    const jitterY: number = Math.min(cellH * 0.25, 60);

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
          x: baseX,
          y: baseY,
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
  }

  function animate(time: number) {
    for (const icon of icons) {
      icon.x = icon.baseX + Math.sin(time * icon.speed + icon.phase) * 12;
      icon.y = icon.baseY + Math.cos(time * icon.speed + icon.phase) * 10;
    }

    icons = icons;

    raf = window.requestAnimationFrame(animate);
  }

  function onResize() {
    const currentWidth: number = window.innerWidth;

    if (Math.abs(currentWidth - lastWidth) < 20) {
      return;
    }

    lastWidth = currentWidth;

    if (resizeTimeout !== null) {
      clearTimeout(resizeTimeout);
    }

    generationOpacity = 0;

    resizeTimeout = window.setTimeout(() => {
      if (raf !== null) {
        cancelAnimationFrame(raf);
        raf = null;
      }

      icons = [];

      requestAnimationFrame(() => {
        generateIcons();
        generationOpacity = 1;
        raf = requestAnimationFrame(animate);
      });

      resizeTimeout = null;
    }, 180);
  }

  onMount(() => {
    generateIcons();
    raf = window.requestAnimationFrame(animate);

    window.addEventListener("resize", onResize);

    return () => {
      window.removeEventListener("resize", onResize);
      if (raf !== null) window.cancelAnimationFrame(raf);
    };
  });

  onDestroy(() => {
    if (raf !== null) window.cancelAnimationFrame(raf);
  });
</script>

<div class="bg-root" bind:this={root} style="opacity: {generationOpacity}">
  {#each icons as icon (icon.id)}
    <img
      src={icon.src}
      alt=""
      class="bg-icon"
      style="
        transform:
          translate3d({icon.x}px, {icon.y}px, 0)
          rotate({icon.rotation}deg)
          scale({icon.scale});
        opacity: {icon.opacity};
      "
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
    filter: blur(4px) saturate(0.75) hue-rotate(-8deg);
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
</style>
