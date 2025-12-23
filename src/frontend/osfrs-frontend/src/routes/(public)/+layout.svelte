<script lang="ts">
  import "../layout.css";
  import favicon from "$lib/assets/favicon.svg";
  import Footer from "$lib/components/Footer.svelte";
  import LandingNavBar from "$lib/components/LandingNavBar.svelte";
  import { page } from "$app/stores";

  const pathname = $derived($page.url.pathname);

  let { children } = $props();
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>

<div class="layout">
  {#if pathname !== "/auth"}
    <LandingNavBar />
  {/if}

  <main class="content">
    {@render children()}
  </main>

  {#if pathname !== "/auth"}
    <Footer />
  {/if}
</div>
