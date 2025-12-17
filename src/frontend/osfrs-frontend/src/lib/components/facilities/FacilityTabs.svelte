<script lang="ts" module>
  export type TabId = "availability" | "gallery" | "reviews";
</script>

<script lang="ts">
  export let active: TabId = "availability";

  const tabs: { id: TabId; label: string }[] = [
    { id: "availability", label: "Availability" },
    { id: "gallery", label: "Gallery" },
    { id: "reviews", label: "Reviews" },
  ];

  function setActive(id: TabId) {
    active = id;
  }
</script>

<nav class="tabs justify-center sm:justify-end">
  {#each tabs as tab (tab.id)}
    <button
      class="tab"
      class:active={active === tab.id}
      on:click={() => setActive(tab.id)}
    >
      {tab.label}
    </button>
  {/each}
</nav>

<style>
  .tabs {
    width: 100%;
    display: flex;
    gap: 1.25rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.08);
    padding-right: 1.25rem;
  }

  .tab {
    background: none;
    border: none;
    padding: 0.75rem 1rem;
    font-size: 0.85rem;
    font-weight: 500;
    color: var(--color-text-secondary, #b3b6bb);
    cursor: pointer;
    position: relative;
    opacity: 0.85;
    transition:
      color 120ms ease,
      opacity 120ms ease;
  }

  .tab:hover {
    color: var(--color-text-primary, #e6e7e9);
    opacity: 1;
  }

  .tab.active {
    color: var(--color-text-primary, #e6e7e9);
    opacity: 1;
  }

  .tab.active::after {
    content: "";
    position: absolute;
    left: 30%;
    right: 30%;
    bottom: 0;
    height: 2px;
    background: linear-gradient(
      90deg,
      transparent,
      rgba(120, 160, 255, 0.9),
      transparent
    );
    border-radius: 999px;
  }
</style>
