<script lang="ts">
  import { SvelteDate } from "svelte/reactivity";
  import { IconCalendarEvent } from "@tabler/icons-svelte";
  import { onMount } from "svelte";

  type SlotStatus = "available" | "booked" | "maintenance";
  type Slot = { id: number; time: string; status: SlotStatus };

  export let anchorDate = new SvelteDate();

  let pickerOpen = false;
  let calendarBtn: HTMLButtonElement | null = null;
  let popX = 0;
  let popY = 0;

  let visibleCount = 3;

  function updateVisibleCount() {
    if (window.matchMedia("(max-width: 767px)").matches) {
      visibleCount = 1;
    } else if (window.matchMedia("(max-width: 1023px)").matches) {
      visibleCount = 2;
    } else {
      visibleCount = 3;
    }
  }

  onMount(() => {
    updateVisibleCount();
    window.addEventListener("resize", updateVisibleCount);
    return () => window.removeEventListener("resize", updateVisibleCount);
  });

  const pad2 = (n: number) => String(n).padStart(2, "0");
  const toISO = (d: Date) =>
    `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;

  function addDays(d: Date, n: number) {
    const x = new SvelteDate(d);
    x.setDate(x.getDate() + n);
    return x;
  }

  $: baseDate = new Date(anchorDate.setHours(0, 0, 0, 0));
  $: days = (() => {
    if (visibleCount === 1) {
      return [baseDate];
    }
    if (visibleCount === 2) {
      return [baseDate, addDays(baseDate, 1)];
    }
    return [addDays(baseDate, -1), baseDate, addDays(baseDate, 1)];
  })();

  const times = [
    "9:00 AM",
    "10:00 AM",
    "11:00 AM",
    "12:00 PM",
    "1:00 PM",
    "2:00 PM",
    "3:00 PM",
    "4:00 PM",
    "5:00 PM",
  ];

  const toCapitalFirst = (s: string) => s && s[0].toUpperCase() + s.slice(1);

  function slotsFor(): Slot[] {
    return times.map((t, i) => {
      if (i % 5 === 0) return { id: i, time: t, status: "maintenance" };
      if (i % 3 === 0) return { id: i, time: t, status: "booked" };
      return { id: i, time: t, status: "available" };
    });
  }

  function shift(direction: number) {
    anchorDate = addDays(anchorDate, direction * visibleCount);
    pickerOpen = false;
  }

  function togglePicker() {
    pickerOpen = !pickerOpen;
    if (calendarBtn) {
      const r = calendarBtn.getBoundingClientRect();
      popX = r.left + r.width / 2;
      popY = r.bottom + 8;
    }
  }

  function pick(e: Event) {
    const v = (e.target as HTMLInputElement).value;
    if (!v) return;
    const [y, m, d] = v.split("-").map(Number);
    anchorDate = new SvelteDate(y, m - 1, d);
    pickerOpen = false;
  }
</script>

<div class="calendar-root">
  <div class="calendar-card">
    <div class="nav">
      <button class="iconbtn" on:click={() => shift(-1)}>←</button>

      <button
        class="iconbtn calbtn"
        bind:this={calendarBtn}
        on:click={togglePicker}
      >
        <IconCalendarEvent size="1rem" />
      </button>

      <button class="iconbtn" on:click={() => shift(1)}>→</button>
    </div>

    <div class="h-divider"></div>

    <div
      class="grid"
      style="grid-template-columns: repeat({visibleCount}, 1fr)"
    >
      {#each days as d, i}
        <section class="day">
          <header class="dayhead p-4">
            <div class="weekday">
              {d.toLocaleDateString(undefined, { weekday: "long" })}
            </div>
            <div class="date">
              {d.toLocaleDateString(undefined, {
                month: "long",
                day: "numeric",
                year: "numeric",
              })}
            </div>
          </header>

          <div class="h-divider"></div>

          <div class="slots p-2">
            {#each slotsFor() as s (s.id)}
              <div class="slot">
                <span class="time">{s.time}</span>
                <span class="pill {s.status}">
                  {toCapitalFirst(s.status)}
                </span>
              </div>
            {/each}
          </div>

          {#if i < visibleCount - 1}
            <div class="divider"></div>
          {/if}
        </section>
      {/each}
    </div>
  </div>

  {#if pickerOpen}
    <div class="popover" style="left:{popX}px; top:{popY}px">
      <input type="date" value={toISO(baseDate)} on:change={pick} />
    </div>
  {/if}
</div>

<style>
  .calendar-root {
    position: relative;
    width: 100%;
  }

  .calendar-card {
    position: relative;
    overflow: hidden;
    margin-bottom: 0.5rem;
  }

  .nav {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem;
  }

  .iconbtn {
    height: 2.1rem;
    width: 2.1rem;
    border-radius: 10px;
    background: rgba(22, 26, 36, 0.55);
    border: 1px solid rgba(255, 255, 255, 0.1);
    color: rgba(235, 240, 250, 0.9);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow:
      inset 0 1px 0 rgba(255, 255, 255, 0.06),
      0 6px 16px rgba(0, 0, 0, 0.4);
  }

  .grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
  }

  @media (max-width: 767px) {
    .grid {
      grid-template-columns: 1fr;
    }

    .day[data-index="0"],
    .day[data-index="2"] {
      display: none;
    }
  }

  @media (min-width: 768px) and (max-width: 1023px) {
    .grid {
      grid-template-columns: repeat(2, 1fr);
    }

    .day[data-index="0"] {
      display: none;
    }
  }

  .day {
    position: relative;
  }

  .divider {
    position: absolute;
    right: 0;
    top: 0;
    width: 1px;
    height: 100%;
    background: linear-gradient(
      180deg,
      transparent,
      rgba(255, 255, 255, 0.14),
      transparent
    );
  }

  .h-divider {
    width: 100%;
    height: 1px;
    background: linear-gradient(
      90deg,
      transparent,
      rgba(255, 255, 255, 0.1),
      transparent
    );
  }

  .weekday {
    font-size: 18px;
    font-weight: 600;
    color: rgba(240, 245, 255, 0.92);
  }

  .date {
    font-size: 12px;
    color: rgba(180, 190, 210, 0.6);
  }

  .slot {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.45rem 0.85rem;
    margin-top: 6px;
    border-radius: 10px;
    background: linear-gradient(
      180deg,
      rgba(14, 18, 26, 0.4),
      rgba(10, 14, 20, 0.45)
    );
    box-shadow:
      inset 0 1px 0 rgba(255, 255, 255, 0.025),
      0 6px 14px rgba(0, 0, 0, 0.4);
  }

  .time {
    color: rgba(230, 235, 245, 0.85);
    font-size: 13px;
  }

  .pill {
    width: 62%;
    height: 2.25rem;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 600;
    letter-spacing: 0.15px;
    user-select: none;
    transition: transform 220ms ease;
    cursor: pointer;
  }

  .pill:hover {
    transform: scale(1.02);
  }

  .available {
    background: rgba(48, 165, 118, 0.32);
    color: rgba(220, 250, 235, 0.95);
  }

  .booked {
    background: rgba(200, 62, 72, 0.42); /* deeper red */
    color: rgba(255, 225, 230, 0.95);
  }

  .maintenance {
    background: #f5930b70;
    color: rgba(255, 245, 220, 0.95);
  }

  .popover {
    position: fixed;
    transform: translateX(-50%);
    background: rgba(18, 22, 32, 0.9);
    backdrop-filter: blur(12px);
    border-radius: 12px;
    padding: 10px;
    border: 1px solid rgba(255, 255, 255, 0.18);
  }
</style>
