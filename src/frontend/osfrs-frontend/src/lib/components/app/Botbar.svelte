<script lang="ts">
  import {
    IconHomeFilled,
    IconUserFilled,
    IconSoccerField,
    IconCalendarCheck,
  } from "@tabler/icons-svelte";
  import { href } from "$lib/nav";

  let { activeIndex } = $props();
</script>

<div class="relative flex flex-row justify-between items-center">
  <div class="indicator-track" style="--i: {activeIndex()}">
    <div class="active-indicator"></div>
  </div>

  <a href={href("/app")} class="bottom-item">
    <IconHomeFilled size="1.6rem" />
    Home
  </a>

  <a href={href("/app/facilities")} class="bottom-item">
    <IconSoccerField size="1.6rem" />
    Facilities
  </a>

  <a href={href("/app/reservations")} class="bottom-item">
    <IconCalendarCheck size="1.6rem" />
    Reservations
  </a>

  <a href={href("/app/profile")} class="bottom-item">
    <IconUserFilled size="1.6rem" />
    Profile
  </a>
</div>

<style>
  .bottom-item {
    position: relative;
    display: flex;
    flex: 1;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    font-size: 10px;
    font-weight: 600;
    transition:
      transform 150ms ease,
      opacity 150ms ease;
  }

  .indicator-track {
    position: absolute;
    bottom: -0.5rem;
    left: 0;

    width: 25%;
    height: 3px;

    transform: translateX(calc(var(--i) * 100%));
    transition: transform 220ms cubic-bezier(0.2, 0.8, 0.2, 1);

    display: flex;
    justify-content: center;
  }

  .active-indicator {
    width: 3rem;
    height: 3px;
    border-radius: 999px;

    background: #3b82f6;
    box-shadow:
      0 0 8px rgba(59, 130, 246, 0.8),
      0 0 16px rgba(59, 130, 246, 0.5);
  }
</style>
