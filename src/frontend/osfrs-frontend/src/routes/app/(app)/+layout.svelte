<script>
  import "../../layout.css";
  import favicon from "$lib/assets/favicon.svg";
  import Sidebar from "$lib/components/app/Sidebar.svelte";

  let { children } = $props();
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>

<div class="app-shell">
  <aside class="sidebar">
    <Sidebar />
  </aside>
  <main class="content">
    {@render children()}
  </main>
</div>
