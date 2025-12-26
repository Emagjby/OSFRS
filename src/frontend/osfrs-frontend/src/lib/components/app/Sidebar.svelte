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
  import { tooltip } from "../ui/Tooltip";
  import { page } from "$app/state";

  let path = $derived(page.route.id);
</script>

<div class="inner-sidebar w-full h-full lg:flex flex-col items-center hidden">
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

<div class="inner-sidebar w-full h-full flex flex-col items-center lg:hidden">
  <a
    href={href("/app")}
    class="text-[12px] flex items-center justify-center w-full font-semibold pt-7 pb-10 tracking-tight"
    >OSFRS</a
  >
  <div
    class="sidenav gap-2 w-[95%] h-[75vh] flex flex-col justify-start items-center"
  >
    {#if path === "/app/(app)"}
      <a
        class="active-path"
        href={href("/app")}
        use:tooltip={{ label: "Home", placement: "right" }}
      >
        <IconHomeFilled size="1.5rem" />
      </a>
    {:else}
      <a
        href={href("/app")}
        use:tooltip={{ label: "Home", placement: "right" }}
      >
        <IconHomeFilled size="1.5rem" />
      </a>
    {/if}

    {#if path === "/app/(app)/facilities"}
      <a
        class="active-path"
        use:tooltip={{ label: "Facilities", placement: "right" }}
        href={href("/app/facilities")}
      >
        <IconSoccerField size="1.5rem" />
      </a>
    {:else}
      <a
        use:tooltip={{ label: "Facilities", placement: "right" }}
        href={href("/app/facilities")}
      >
        <IconSoccerField size="1.5rem" />
      </a>
    {/if}

    {#if path === "/app/(app)/reservations"}
      <a
        use:tooltip={{ label: "My Reservations", placement: "right" }}
        class="active-path"
        href={href("/app/reservations")}
      >
        <IconCalendarCheck size="1.5rem" />
      </a>
    {:else}
      <a
        use:tooltip={{ label: "My Reservations", placement: "right" }}
        href={href("/app/reservations")}
      >
        <IconCalendarCheck size="1.5rem" />
      </a>
    {/if}

    {#if path === "/app/(app)/profile"}
      <a
        use:tooltip={{ label: "Profile", placement: "right" }}
        class="active-path"
        href={href("/app/profile")}
      >
        <IconUserFilled size="1.5rem" />
      </a>
    {:else}
      <a
        use:tooltip={{ label: "Profile", placement: "right" }}
        href={href("/app/profile")}
      >
        <IconUserFilled size="1.5rem" />
      </a>
    {/if}
  </div>
  <div class="sidelogout w-full flex flex-col items-center justify-end h-full">
    <Separator />
    <a
      class="my-4 lg:w-[90%] lg:aspect-auto w-[75%] aspect-square"
      href={href("/logout")}
      use:tooltip={{ label: "Logout", placement: "right" }}
    >
      <IconDoorExit size="1.5rem" />
    </a>
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
    aspect-ratio: auto;
  }

  @media (max-width: 1023px) {
    .sidenav > a,
    .sidelogout > a {
      justify-content: center;
      padding: 0.6rem 0;
      width: 75%;
      aspect-ratio: 1/1;
    }
  }

  .sidenav > a:hover,
  .sidelogout > a:hover {
    background: rgba(255, 255, 255, 0.06);
  }

  @media (min-width: 1024px) {
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
  }

  @media (max-width: 1023px) {
    .active-path {
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid var(--color-border-soft);
      box-shadow:
        0 8px 30px rgba(0, 0, 0, 0.35),
        0 1px 0 rgba(255, 255, 255, 0.04) inset;
    }
  }

  .inner-sidebar {
    background: rgba(0, 0, 0, 0.15);
  }
</style>
