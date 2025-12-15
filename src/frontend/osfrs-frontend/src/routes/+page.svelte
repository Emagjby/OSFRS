<script lang="ts">
  import Hero from "$lib/components/Hero.svelte";
  import FacilitiesPreview from "$lib/components/FacilitiesPreview.svelte";
  import FloatingBackground from "$lib/components/FloatingBackground.svelte";
</script>

<FloatingBackground />

<main class="relative z-10">
  <Hero />
  <FacilitiesPreview />
</main>
