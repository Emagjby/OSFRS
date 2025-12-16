<script>
  import { href } from "$lib/nav";

  const { id, name, status, src } = $props();
</script>

<div class="facility-card w-full h-30" style="--bg: url('{src}')">
  <h3>{name}</h3>
  <p>{status}</p>
  <a href={href(`/facility/${id}`)}>View Availability</a>
</div>

<style>
  .facility-card {
    background-image: var(--bg);
    background-size: 100%;
  }
</style>
