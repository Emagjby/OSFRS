<script lang="ts">
  import { onMount } from "svelte";

  onMount(async () => {
    const { tsParticles } = await import("tsparticles-engine");
    const { loadSlim } = await import("tsparticles-slim");

    await loadSlim(tsParticles);

    tsParticles.load("bg", {
      fullScreen: { enable: false },
      background: { color: "transparent" },

      particles: {
        number: {
          value: 12,
          density: {
            enable: true,
            area: 1400,
          },
        },

        shape: {
          type: "image",
          image: [
            {
              src: "/icon1.svg",
              width: 96,
              height: 96,
            },
            {
              src: "/icon2.svg",
              width: 96,
              height: 96,
            },
            {
              src: "/icon3.svg",
              width: 96,
              height: 96,
            },
            {
              src: "/icon4.svg",
              width: 96,
              height: 96,
            },
            {
              src: "/icon5.svg",
              width: 96,
              height: 96,
            },
          ],
        },

        size: {
          value: 72,
          random: {
            enable: true,
            minimumValue: 44,
          },
        },

        opacity: {
          value: 0.28,
          random: {
            enable: true,
            minimumValue: 0.08,
          },
        },

        move: {
          enable: true,
          speed: 0.2,
          direction: "none",
          random: false,
          straight: false,

          outModes: {
            default: "bounce",
          },
        },

        collisions: {
          enable: true,
          mode: "bounce",
        },
      },

      detectRetina: true,
    });
  });
</script>

<div id="bg" class="fixed inset-0 z-0 pointer-events-none"></div>

<style>
  :global(#bg canvas) {
    filter: blur(4px) saturate(0.75) hue-rotate(-8deg);
    transform: scale(1.05);
    opacity: 0.48;
  }
</style>
