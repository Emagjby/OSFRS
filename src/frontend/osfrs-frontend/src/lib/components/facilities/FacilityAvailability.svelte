<script lang="ts">
  import {
    IconCalendarEvent,
    IconChevronLeft,
    IconChevronRight,
  } from "@tabler/icons-svelte";
  import { SvelteDate } from "svelte/reactivity";

  type SlotStatus = "available" | "booked" | "maintenance";
  type Slot = { id: number; time: string; status: SlotStatus };

  let anchorDate = new SvelteDate();
  let pickerOpen = $state(false);
  let calendarBtn: HTMLButtonElement | null = $state(null);
  let popX = $state(0);
  let popY = $state(0);
  let visibleCount = $state(3);

  $effect(() => {
    const update = () => {
      if (window.matchMedia("(max-width: 767px)").matches) visibleCount = 1;
      else if (window.matchMedia("(max-width: 1023px)").matches)
        visibleCount = 2;
      else visibleCount = 3;
    };
    update();
    window.addEventListener("resize", update);
    return () => window.removeEventListener("resize", update);
  });

  const baseDate = $derived.by(() => {
    const d = new SvelteDate(anchorDate.getTime());
    d.setHours(0, 0, 0, 0);
    return d;
  });

  const days = $derived.by(() => {
    const b = baseDate;
    if (visibleCount === 1) return [b];
    if (visibleCount === 2) return [b, addDays(b, 1)];
    return [addDays(b, -1), b, addDays(b, 1)];
  });

  const pad2 = (n: number) => String(n).padStart(2, "0");

  const toISO = (d: SvelteDate) =>
    `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;

  function addDays(d: SvelteDate, n: number) {
    const x = new SvelteDate(d.getTime());
    x.setDate(x.getDate() + n);
    return x;
  }

  const toCapitalFirst = (str: string) =>
    str.length ? str[0].toUpperCase() + str.slice(1) : str;

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

  function slotsFor(): Slot[] {
    return times.map((t, i) => {
      if (i % 5 === 0) return { id: i, time: t, status: "maintenance" };
      if (i % 3 === 0) return { id: i, time: t, status: "booked" };
      return { id: i, time: t, status: "available" };
    });
  }

  function shift(direction: number) {
    anchorDate.setDate(anchorDate.getDate() + direction * visibleCount);
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
    anchorDate.setFullYear(y, m - 1, d);
    pickerOpen = false;
  }
</script>

<div class="calendar-root">
  <div class="calendar-card">
    <div class="nav">
      <button class="iconbtn" onclick={() => shift(-1)} aria-label="Previous"
        ><IconChevronLeft size="1rem" /></button
      >

      <button
        class="iconbtn calbtn"
        bind:this={calendarBtn}
        onclick={togglePicker}
        aria-label="Open Calendar"
      >
        <IconCalendarEvent size="1rem" />
      </button>

      <button class="iconbtn" onclick={() => shift(1)} aria-label="Next"
        ><IconChevronRight size="1rem" /></button
      >
    </div>

    <div class="h-divider"></div>

    <div
      class="grid"
      style="grid-template-columns: repeat({visibleCount}, 1fr)"
    >
      {#each days as d (d.getTime())}
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

          {#if days.indexOf(d) < visibleCount - 1}
            <div class="divider"></div>
          {/if}
        </section>
      {/each}
    </div>
  </div>

  {#if pickerOpen}
    <div class="popover" style="left:{popX}px; top:{popY}px">
      <input type="date" value={toISO(baseDate)} onchange={pick} />
    </div>
  {/if}
</div>

<style>
  .calendar-root {
    position: relative;
    width: 100%;
    font-family: sans-serif;
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
    transition: border-color 220ms ease;
  }

  .iconbtn:hover {
    border-color: rgba(255, 255, 255, 0.3);
  }

  .grid {
    display: grid;
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

  .dayhead {
    text-align: center;
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
    background: rgba(200, 62, 72, 0.42);
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
    z-index: 100;
  }

  .p-2 {
    padding: 0.5rem;
  }

  .p-4 {
    padding: 1rem;
  }
</style>
