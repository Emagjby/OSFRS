<script lang="ts">
  import { href } from "$lib/nav";
</script>

<div class="flex flex-row justify-between p-4 max-w-6xl mx-auto" id="navbar">
  <div class="flex flex-row gap-4" id="left-navbar">
    <p class="font-semibold text-xl">OSFRS</p>
  </div>

  <div class="flex flex-row gap-4" id="right-navbar">
    <a
      class="text-lg border-gray-700 border"
      href={href("/auth", new URLSearchParams({ mode: "login" }))}>Login</a
    >
    <a
      class="text-lg"
      href={href("/auth", new URLSearchParams({ mode: "register" }))}
      >Register</a
    >
  </div>
</div>
