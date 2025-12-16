<script lang="ts">
  import { href } from "$lib/nav";
</script>

<div
  class="flex flex-col justify-center items-center min-h-[min(85vh,700px)] gap-4 mx-auto py-12 px-6 w-full"
>
  <div>
    <h1
      class="text-center text-[1.675rem] xs:text-4xl lg:text-5xl max-w-sm xs:max-w-xl lg:max-w-3xl font-semibold leading-tight"
    >
      Online Sports Facility Reservation System
    </h1>
  </div>
  <div>
    <h2
      class="text-center text-sm xs:text-lg lg:text-xl font-light max-w-3xs xs:max-w-xs lg:max-w-md text-text-secondary"
    >
      Explore and reserve different types of sports facilities with ease.
    </h2>
  </div>
  <div
    class="my-8 flex flex-col xs:flex-row justify-around items-center gap-4 w-full xs:w-auto"
  >
    <a
      class="flex justify-center items-center font-normal btn-neutral border-success-secondary bg-success text-md xs:text-lg px-6 py-3 w-full xs:w-auto"
      href={href("/facilities")}>Get Started</a
    >
    <a
      class="flex justify-center items-center font-medium btn-neutral text-md xs:text-lg px-6 py-3 w-full xs:w-auto"
      href={href("/availability")}>Browse Facilities</a
    >
  </div>
</div>
