<script lang="ts">
  import "../../layout.css";
  import favicon from "$lib/assets/favicon.svg";
  import Sidebar from "$lib/components/app/Sidebar.svelte";
  import Botbar from "$lib/components/app/Botbar.svelte";

  import { page } from "$app/state";

  const { children } = $props();

  const path = $derived(page.route.id);

  const tabs = [
    "/app/(app)",
    "/app/(app)/facilities",
    "/app/(app)/reservations",
    "/app/(app)/profile",
  ];

  const activeIndex = $derived(() => {
    const i = tabs.indexOf(path ?? "/app/(app)");
    return i === -1 ? 0 : i;
  });
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>

<div class="app-shell">
  <aside class="sidebar hidden xs:flex">
    <Sidebar />
  </aside>
  <main class="content pb-[calc(64px+env(safe-area-inset-bottom))] xs:pb-0">
    {@render children()}
  </main>
  <nav
    class="h-16 fixed bottom-0 left-0 right-0 z-50 xs:hidden bg-[rgba(0,0,0,0.15)] border-t border-border-soft py-2 pb-[calc(0.5rem+env(safe-area-inset-bottom))]"
  >
    <Botbar {activeIndex} />
  </nav>
</div>
