<script lang="ts">
  import FacilityTabs from "$lib/components/facilities/FacilityTabs.svelte";

  //will be gotten from the page option
  const mockData = {
    name: "Kristal",
    type: "Gym",
    status: "Available",
    src: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
  };
</script>

<main class="max-w-6xl m-auto p-4">
  <div class="facility-card flex flex-col">
    <h2 class="text-2xl font-semibold mt-8 ml-8 text-text-primary">
      {mockData.name}
    </h2>
    <h3 class="text-sm font-light ml-8 text-text-secondary pb-4">
      {mockData.type}
    </h3>
    <p class="status {mockData.status.toLowerCase()} ml-8">{mockData.status}</p>
    <FacilityTabs />

    <p>Lorem ipsum pedal</p>
  </div>
</main>
