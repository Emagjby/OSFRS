<script lang="ts">
  const srcs = [
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkarA_0L4R1NYltv8EEyaeKBoIA53bzLJFxg&s",
  ];
</script>

<div class="min-h-160">
  <div
    class="gallery-wrapper grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4"
  >
    {#each srcs as src, i (i)}
      <div class="image-wrapper">
        <img
          {src}
          alt="Facility"
          loading="lazy"
          class="w-full h-full object-cover block"
        />
      </div>
    {/each}
  </div>
</div>

<style>
  .gallery-wrapper {
    display: grid;
    padding: 1rem;
    gap: 1rem;
  }

  .image-wrapper {
    border-radius: 0.5rem;
    border: 1px solid rgba(255, 255, 255, 0.55);
    position: relative;
    width: 100%;
    aspect-ratio: 16 / 9;
    overflow: hidden;

    transition: transform 220ms ease;
  }

  .image-wrapper:hover {
    transform: scale(1.05);
  }
</style>
