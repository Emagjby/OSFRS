<script lang="ts">
  import FacilityTabs, {
    type TabId,
  } from "$lib/components/facilities/FacilityTabs.svelte";
  import FacilityAvailability from "$lib/components/facilities/FacilityAvailability.svelte";
  import { IconAlertTriangleFilled } from "@tabler/icons-svelte";

  let activeTab: TabId = "availability";

  //will be gotten from the page option
  const mockData = {
    name: "Kristal",
    type: "Gym",
    status: "Available",
    src: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
    incomingMaintenences: [
      {
        start: new Date(),
        end: new Date(Date.now() + 12 * 60 * 60 * 1000),
        description: "Fixing light bulbs flickering...",
      },
    ],
  };
</script>

<main class="max-w-6xl m-auto p-4">
  <div class="facility-page-card flex flex-col">
    <div id="facility-top" class="flex flex-col sm:flex-row justify-between">
      <div id="facility-top-left">
        <h2 class="text-2xl font-semibold mt-8 ml-8 text-text-primary">
          {mockData.name}
        </h2>
        <h3 class="text-sm font-light ml-8 text-text-secondary pb-4">
          {mockData.type}
        </h3>
        <p class="status {mockData.status.toLowerCase()} ml-8">
          {mockData.status}
        </p>
      </div>
      <div>
        {#if mockData.incomingMaintenences.length}
          <p class="incoming-maintenance">
            <IconAlertTriangleFilled size="1.5rem" />
            Incoming Maintenance!
          </p>
        {/if}
      </div>
    </div>
    <FacilityTabs bind:active={activeTab} />
    {#if activeTab === "availability"}
      <FacilityAvailability />
    {:else if activeTab === "gallery"}
      <p>galerry</p>
    {:else}
      <p>reviews</p>
    {/if}
  </div>
</main>

<style>
  .incoming-maintenance {
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    gap: 0.5rem;
    margin: 2rem;
    color: #f5930b;
    font-weight: var(--font-weight-semibold);

    transition: transform 220ms ease;

    will-change: transform;
  }

  .incoming-maintenance:hover {
    transform: scale(1.02);
  }
</style>
