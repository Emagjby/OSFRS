<script lang="ts">
  import FacilityCard from "./FacilityCard.svelte";

  const mockData = [
    {
      id: 1,
      name: "Gym",
      status: "Availabale",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
    {
      id: 2,
      name: "Gym",
      status: "Availabale",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
    {
      id: 3,
      name: "Gym",
      status: "Availabale",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
    {
      id: 4,
      name: "Gym",
      status: "Availabale",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
    {
      id: 5,
      name: "Gym",
      status: "Availabale",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
    {
      id: 6,
      name: "Gym",
      status: "Availabale",
      src: "https://gymleco.com/cdn/shop/articles/Namnlos-design-11-1024x1024_b903e121-27ca-42b7-b44b-f8802a3485dd.png?v=1737651463&width=1024",
    },
  ];
</script>

<div
  id="facilities-preview"
  class="flex flex-col justify-center items-start gap-4 max-w-6xl mx-auto py-12 px-4 w-full"
>
  <h3 class="text-2xl font-semibold">Explore Facilities</h3>
  <div id="facility-cards" class="grid grid-cols-3 w-full">
    {#each mockData as facility (facility.id)}
      <FacilityCard
        id={facility.id}
        name={facility.name}
        status={facility.status}
        src={facility.src}
      />
    {/each}
  </div>
</div>
