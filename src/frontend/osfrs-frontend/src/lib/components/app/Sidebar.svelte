<script lang="ts">
  import { href } from "$lib/nav";
  import Separator from "../ui/separator/Separator.svelte";
  import {
    IconHomeFilled,
    IconSoccerField,
    IconCalendarCheck,
    IconUserFilled,
    IconDoorExit,
  } from "@tabler/icons-svelte";
  import { page } from "$app/state";

  let path = $derived(page.route.id);
</script>

<div class="inner-sidebar w-full h-full flex flex-col items-center">
  <a href={href("/app")} class="text-3xl font-semibold pt-7 pb-10 tracking-wide"
    >OSFRS</a
  >
  <div class="sidenav gap-2 w-[95%] h-[75vh] flex flex-col justify-start">
    {#if path === "/app/(app)"}
      <a class="active-path" href={href("/app")}
        ><IconHomeFilled size="1.5rem" /> Home</a
      >
    {:else}
      <a href={href("/app")}><IconHomeFilled size="1.5rem" /> Home</a>
    {/if}

    {#if path === "/app/(app)/facilities"}
      <a class="active-path" href={href("/app/facilities")}
        ><IconSoccerField size="1.5rem" />Facilities</a
      >
    {:else}
      <a href={href("/app/facilities")}
        ><IconSoccerField size="1.5rem" />Facilities</a
      >
    {/if}

    {#if path === "/app/(app)/reservations"}
      <a class="active-path" href={href("/app/reservations")}
        ><IconCalendarCheck size="1.5rem" />My Reservations</a
      >
    {:else}
      <a href={href("/app/reservations")}
        ><IconCalendarCheck size="1.5rem" />My Reservations</a
      >
    {/if}

    {#if path === "/app/(app)/profile"}
      <a class="active-path" href={href("/app/profile")}
        ><IconUserFilled size="1.5rem" />Profile</a
      >
    {:else}
      <a href={href("/app/profile")}><IconUserFilled size="1.5rem" />Profile</a>
    {/if}
  </div>
  <div class="sidelogout w-full flex flex-col items-center justify-end h-full">
    <Separator />
    <a class="my-4 w-[95%]" href={href("/logout")}
      ><IconDoorExit size="1.5rem" />Logout</a
    >
  </div>
</div>

<style>
  .sidenav > a,
  .sidelogout > a {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    font-size: 1rem;
    padding: 0.6rem 1.4rem;
    border-radius: 0.35rem;
    transition:
      background 150ms ease,
      border-color 150ms ease;
  }

  .sidenav > a:hover,
  .sidelogout > a:hover {
    background: rgba(255, 255, 255, 0.06);
  }

  .active-path {
    position: relative;
    background: rgba(255, 255, 255, 0.04);
    border-top-left-radius: 0 !important;
    border-bottom-left-radius: 0 !important;
    margin-left: -0.5rem;
    border: 1px solid var(--color-border-soft);
  }

  .active-path::before {
    content: "";
    position: absolute;
    left: 0;
    top: 50%;
    transform: translateY(-50%);
    height: 100%;
    width: 0.3rem;
    background: rgb(59 130 246);
    box-shadow: 0 0 12px rgba(59, 130, 246, 0.6);
  }

  .inner-sidebar {
    background: rgba(0, 0, 0, 0.15);
  }
</style>
