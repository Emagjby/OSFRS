<script lang="ts">
  import { SvelteDate } from "svelte/reactivity";
  import { IconCalendarEvent } from "@tabler/icons-svelte";

  type SlotStatus = "available" | "booked" | "maintenance";
  type Slot = { hour: string; status: SlotStatus };

  let currentDate = new SvelteDate();
  let dateInput = $state<HTMLInputElement | null>(null);
  let calendarOpen = $state(false);

  const formatDate = (date: Date) =>
    date.toLocaleDateString("en-US", {
      weekday: "long",
      month: "long",
      day: "numeric",
      year: "numeric",
    });

  const isToday = (date: Date) => {
    const now = new Date();
    return (
      date.getFullYear() === now.getFullYear() &&
      date.getMonth() === now.getMonth() &&
      date.getDate() === now.getDate()
    );
  };

  const dateLabel = $derived(isToday(currentDate) ? "Today" : "");
  const fullDate = $derived(formatDate(currentDate));

  function isoDate(d: Date) {
    return d.toISOString().slice(0, 10);
  }

  function prevDay() {
    currentDate.setDate(currentDate.getDate() - 1);
  }

  function nextDay() {
    currentDate.setDate(currentDate.getDate() + 1);
  }

  function gotoRegister() {
    window.location.href = "/auth?mode=register";
  }

  function toggleCalendar() {
    calendarOpen = !calendarOpen;

    if (calendarOpen) {
      requestAnimationFrame(() => {
        if (dateInput) {
          dateInput.value = isoDate(currentDate);
          dateInput.showPicker?.();
        }
      });
    }
  }

  function onDatePick(e: Event) {
    const value = (e.target as HTMLInputElement).value;
    if (!value) return;

    const picked = new Date(value);
    currentDate.setFullYear(picked.getFullYear());
    currentDate.setMonth(picked.getMonth());
    currentDate.setDate(picked.getDate());

    calendarOpen = false;
  }

  const {
    slots = [
      { hour: "6:00 AM", status: "available" },
      { hour: "7:00 AM", status: "available" },
      { hour: "9:00 AM", status: "booked" },
      { hour: "10:00 AM", status: "maintenance" },
      { hour: "11:00 AM", status: "available" },
      { hour: "12:00 PM", status: "available" },
      { hour: "1:00 PM", status: "available" },
      { hour: "2:00 PM", status: "available" },
      { hour: "3:00 PM", status: "available" },
      { hour: "4:00 PM", status: "available" },
      { hour: "5:00 PM", status: "available" },
      { hour: "6:00 PM", status: "available" },
    ],
  } = $props<{ slots?: Slot[] }>();
</script>

<div class="schedule mb-4">
  <header class="header">
    <button class="nav" onclick={prevDay}>‹</button>

    <div class="date">
      <div class="date-row">
        {#if dateLabel}
          <span class="today">{dateLabel}</span>
        {/if}
        <span class="full">{fullDate}</span>

        <div class="calendar-wrapper">
          <button
            class="calendar-btn"
            onclick={toggleCalendar}
            aria-label="Open calendar"
          >
            <IconCalendarEvent size="1rem" />
          </button>

          {#if calendarOpen}
            <input
              bind:this={dateInput}
              type="date"
              class="calendar-input"
              oninput={onDatePick}
            />
          {/if}
        </div>
      </div>
    </div>

    <button class="nav" onclick={nextDay}>›</button>
  </header>

  <div class="grid">
    {#each slots as slot (slot.hour)}
      <div class="row">
        <div class="hour">{slot.hour}</div>

        <div
          class="slot {slot.status}"
          role="button"
          tabindex="0"
          onclick={gotoRegister}
          aria-label="Book time slot"
          onkeydown={(e) => e.key === "Enter" && gotoRegister()}
        >
          {#if slot.status === "available"}
            Available
          {:else if slot.status === "booked"}
            <span class="dot red"></span> Booked
          {:else}
            <span class="dot orange"></span> Maintenance
          {/if}
        </div>
      </div>
    {/each}
  </div>
</div>

<style>
  .schedule {
    border-radius: 14px;
    padding: 16px;
    backdrop-filter: blur(12px);
    color: #e6e7e9;
    width: 100%;
  }

  .header {
    display: grid;
    grid-template-columns: auto 1fr auto;
    align-items: center;
    margin-bottom: 12px;
  }

  .nav {
    background: rgba(255, 255, 255, 0.06);
    border: none;
    border-radius: 8px;
    width: 36px;
    height: 36px;
    color: #ccc;
    cursor: pointer;
  }

  .date {
    text-align: center;
  }

  .date-row {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    flex-wrap: wrap;
    justify-content: center;
  }

  .today {
    font-weight: 600;
    font-size: 0.9rem;
  }

  .full {
    color: #9aa0a6;
    font-size: 0.9rem;
  }

  .calendar-wrapper {
    position: relative;
  }

  .calendar-btn {
    background: rgba(255, 255, 255, 0.08);
    border: none;
    border-radius: 6px;
    width: 28px;
    height: 28px;
    color: #ccc;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .calendar-btn:hover {
    background: rgba(255, 255, 255, 0.15);
  }

  .calendar-input {
    position: absolute;
    top: calc(100% + 6px);
    left: 0;
    opacity: 0;
    pointer-events: none;
  }

  .grid {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  .row {
    display: grid;
    grid-template-columns: 80px 1fr;
    align-items: center;
    gap: 12px;
  }

  .hour {
    color: #9aa0a6;
    font-size: 0.85rem;
  }

  .slot {
    min-height: 44px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    font-size: 0.95rem;
    transition:
      transform 160ms ease,
      box-shadow 160ms ease,
      filter 160ms ease;
  }

  .slot:hover {
    transform: translateY(-1px) scale(1.01);
    filter: brightness(1.08);
  }

  .available {
    background: linear-gradient(
      to bottom,
      rgba(70, 120, 90, 0.55),
      rgba(50, 90, 70, 0.55)
    );
    color: #dff3e8;
  }

  .booked {
    background: linear-gradient(
      to bottom,
      rgba(120, 60, 70, 0.7),
      rgba(90, 40, 50, 0.7)
    );
    color: #ffd9de;
  }

  .maintenance {
    background: linear-gradient(
      to bottom,
      rgba(150, 120, 70, 0.7),
      rgba(120, 95, 55, 0.7)
    );
    color: #fff1d6;
  }

  .dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    margin-right: 8px;
  }

  .red {
    background: #ff6b81;
  }

  .orange {
    background: #f4a261;
  }

  @media (max-width: 640px) {
    .header {
      grid-template-columns: 1fr;
      gap: 8px;
    }

    .nav {
      width: 100%;
      height: 40px;
    }

    .row {
      grid-template-columns: 64px 1fr;
    }

    .slot {
      min-height: 52px;
      font-size: 1rem;
    }

    .hour {
      font-size: 0.8rem;
    }
  }
</style>
