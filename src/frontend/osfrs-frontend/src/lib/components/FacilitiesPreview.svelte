<script lang="ts">
  import FacilityCard from "./FacilityCard.svelte";
  import { page } from "$app/stores";

  $: currentPath = $page.url.pathname;

  const mockData = [
    {
      id: 1,
      name: "Gym",
      status: "Available",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
    {
      id: 2,
      name: "Gym",
      status: "Under Maintenance",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
    {
      id: 3,
      name: "Gym",
      status: "Unavailable",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
  ];
</script>

<div
  id="facilities-preview"
  class="flex flex-col justify-center items-start gap-8 max-w-6xl mx-auto py-12 px-4 w-full"
>
  {#if currentPath === "/"}
    <h3 class="text-3xl font-semibold">Explore Facilities</h3>
  {:else if currentPath === "/facilities"}
    <div>
      <h3 class="text-3xl font-semibold">Facilities</h3>
      <p class="text-text-secondary">
        Browse available sports facilities and check their schedules
      </p>
    </div>
  {/if}
  <div
    id="facility-cards"
    class="grid gird-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 w-full h-full"
  >
    {#each mockData as facility (facility.id)}
      <FacilityCard
        id={facility.id}
        name={facility.name}
        status={facility.status}
        src={facility.src}
      />
    {/each}
    {#if currentPath === "/facilities"}
      {#each mockData as facility (facility.id)}
        <FacilityCard
          id={facility.id}
          name={facility.name}
          status={facility.status}
          src={facility.src}
        />
      {/each}
    {/if}
  </div>
</div>
