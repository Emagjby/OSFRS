<script lang="ts">
  import { href } from "$lib/nav";
</script>

<footer class="flex flex-col max-w-6xl mx-auto p-4 pb-8 mt-auto">
  <!-- TOP -->
  <div
    id="footer-top"
    class="w-full flex flex-col items-center gap-4
           md:flex-row md:justify-between md:items-end"
  >
    <!-- LEFT -->
    <div
      id="ft-left"
      class="flex flex-col leading-tight text-center md:text-left"
    >
      <h3 class="text-xl font-semibold">OSFRS</h3>
      <h4 class="text-sm text-text-secondary">Reserve Sports Facilities</h4>
    </div>

    <!-- RIGHT -->
    <div
      id="ft-right"
      class="flex flex-wrap justify-center gap-6
             md:flex-nowrap md:justify-end md:gap-8"
    >
      <a href={href("/facilities")} class="text-sm text-text-secondary">
        Facilities
      </a>
      <a href={href("/about")} class="text-sm text-text-secondary"> About </a>
      <a href={href("/contact")} class="text-sm text-text-secondary">
        Contact
      </a>
    </div>
  </div>

  <!-- DIVIDER -->
  <div id="footer-line"></div>

  <!-- BOTTOM -->
  <div
    id="footer-bot"
    class="w-full flex flex-col items-center gap-3 text-center
           md:flex-row md:justify-between md:text-left"
  >
    <div id="fb-left">
      <p class="text-sm text-text-disabled">
        &copy; {new Date().getFullYear()} OSFRS. All rights reserved.
      </p>
    </div>

    <div id="fb-right" class="flex gap-6">
      <a href={href("/privacy")} class="text-sm text-text-disabled">
        Privacy Policy
      </a>
      <a href={href("/tos")} class="text-sm text-text-disabled">
        Terms of Service
      </a>
    </div>
  </div>
</footer>

<style>
  #footer-line {
    width: 100%;
    height: 1px;
    margin: 1.25rem 0;
    background: linear-gradient(
      to right,
      transparent,
      rgba(255, 255, 255, 0.25),
      transparent
    );
    opacity: 0.6;
  }

  a {
    transition:
      color 150ms ease,
      transform 220ms ease;
  }

  @media (hover: hover) {
    a:hover {
      color: var(--color-text-primary);
      transform: translateY(-1px);
    }
  }
</style>
