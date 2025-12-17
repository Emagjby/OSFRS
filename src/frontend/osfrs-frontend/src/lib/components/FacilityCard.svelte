<script lang="ts">
  import { onMount } from "svelte";
  import { href } from "$lib/nav";

  const { id, name, status, src } = $props();

  let cardEl: HTMLDivElement;

  onMount(() => {
    const img = new Image();
    img.decoding = "async";
    img.src = src;

    img.onload = () => {
      cardEl.style.setProperty("--bg", `url("${img.src}")`);
    };
  });
</script>

<div class="facility-card w-full aspect-square" bind:this={cardEl}>
  <div class="facility-overlay">
    <h4>{name}</h4>

    {#if status === "Available"}
      <p class="status available">
        {status}
      </p>
    {:else if status === "Under Maintenance"}
      <p class="status maintenance">
        {status}
      </p>
    {:else}
      <p class="status unavailable">
        {status}
      </p>
    {/if}

    <a
      class="btn-neutral text-[1.05rem] text-center mt-3"
      href={href(`/facilities/${id}`)}
    >
      View Availability
    </a>
  </div>
</div>
