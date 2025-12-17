<script lang="ts">
  import "./layout.css";
  import favicon from "$lib/assets/favicon.svg";
  import Footer from "$lib/components/Footer.svelte";
  import LandingNavBar from "$lib/components/LandingNavBar.svelte";

  let { children } = $props();
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>

<div class="layout">
  <LandingNavBar />

  <main class="content">
    {@render children()}
  </main>

  <Footer />
</div>
