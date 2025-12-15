<script lang="ts">
  import { href } from "$lib/nav";
</script>

<div class="flex flex-row justify-between p-4 max-w-6xl mx-auto" id="navbar">
  <div class="flex flex-row gap-4" id="left-navbar">
    <a class="logo-glass font-semibold text-xl cursor-pointer" href={href("/")}
      >OSFRS</a
    >
  </div>

  <div class="flex flex-row gap-4" id="right-navbar">
    <a
      class="btn-neutral bg-register-fill"
      href={href("/auth", new URLSearchParams({ mode: "login" }))}>Login</a
    >
    <a
      class="btn-neutral"
      href={href("/auth", new URLSearchParams({ mode: "register" }))}
      >Register</a
    >
  </div>
</div>
