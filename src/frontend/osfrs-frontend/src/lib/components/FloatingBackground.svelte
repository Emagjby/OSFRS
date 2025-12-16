<script lang="ts">
  import { onMount, onDestroy } from "svelte";

  type Icon = {
    baseX: number;
    baseY: number;
    x: number;
    y: number;
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

  const GRID_SPACING = 320;
  const JITTER_X = 40;
  const JITTER_Y = 40;

  const GRID_OFFSET_X = 0.25;
  const GRID_OFFSET_Y = 0.25;

  let root: HTMLDivElement;

  let icons: Icon[] = [];
  let raf: number | null = null;

  const rand = (min: number, max: number) => min + Math.random() * (max - min);

  function generateIcons() {
    const vw = window.innerWidth;
    const docH = document.documentElement.scrollHeight;

    if (root) {
      root.style.height = `${docH}px`;
    }

    const cols = Math.max(1, Math.floor(vw / GRID_SPACING));
    const rows = Math.max(1, Math.floor(docH / GRID_SPACING));

    const result: Icon[] = [];

    for (let row = 0; row < rows; row++) {
      for (let col = 0; col < cols; col++) {
        const cellW = vw / cols;
        const cellH = docH / rows;

        const baseX =
          col * cellW + cellW * GRID_OFFSET_X + rand(-JITTER_X, JITTER_X);

        const baseY =
          row * cellH + cellH * GRID_OFFSET_Y + rand(-JITTER_Y, JITTER_Y);

        result.push({
          baseX,
          baseY,
          x: baseX,
          y: baseY,
          speed: rand(0.0004, 0.001),
          phase: Math.random() * Math.PI * 2,
          scale: rand(0.6, 1.0),
          opacity: rand(0.18, 0.38),
          src: ICONS[Math.floor(Math.random() * ICONS.length)],
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

  onMount(() => {
    generateIcons();
    raf = window.requestAnimationFrame(animate);

    const onResize = () => {
      if (raf !== null) {
        window.cancelAnimationFrame(raf);
      }
      generateIcons();
      raf = window.requestAnimationFrame(animate);
    };

    window.addEventListener("resize", onResize);

    return () => {
      window.removeEventListener("resize", onResize);
      if (raf !== null) {
        window.cancelAnimationFrame(raf);
      }
    };
  });

  onDestroy(() => {
    if (raf !== null) {
      window.cancelAnimationFrame(raf);
    }
  });
</script>

<div class="bg-root bind:this={root}">
  {#each icons as icon}
    <img
      src={icon.src}
      alt=""
      class="bg-icon"
      style="
        transform:
          translate3d({icon.x}px, {icon.y}px, 0)
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
