<script lang="ts">
  import LoginForm from "$lib/components/auth/LoginForm.svelte";
  import { href } from "$lib/nav";
</script>

<div class="min-h-svh">
  <div class="grid min-h-svh lg:grid-cols-2">
    <div
      class="
        relative flex flex-col gap-6 p-6 md:p-10
        overflow-hidden
        bg-[radial-gradient(900px_circle_at_25%_15%,rgba(60,120,165,0.20),transparent_55%)]
        lg:border-r lg:border-white/10
      "
    >
      <div
        class="pointer-events-none absolute inset-0 opacity-[0.08] mix-blend-overlay"
        style="background-image: url('/noise.png'); background-size: 280px 280px;"
      ></div>

      <div class="relative flex justify-center gap-2 md:justify-start">
        <a
          href={href("/")}
          class="flex items-center gap-2 font-semibold text-xl text-white/90"
        >
          OSFRS
        </a>
      </div>

      <div class="relative flex flex-1 items-center justify-center">
        <div
          class="
      w-full max-w-sm
      rounded-2xl border border-white/10
      bg-white/4 backdrop-blur-xl
      shadow-[0_22px_70px_rgba(0,0,0,0.55)]
      p-8
    "
        >
          <LoginForm />
        </div>
      </div>
    </div>

    <div class="relative hidden lg:block overflow-hidden">
      <img
        src="/banner.png"
        loading="lazy"
        alt="OSFRS banner"
        class="
          absolute inset-0 h-full w-full object-cover
          brightness-[0.75] contrast-[1.1] saturate-[1.5]
          scale-[1.03]
        "
      />

      <div
        class="absolute inset-0 bg-gradient-to-br from-[#081220]/70 via-[#081220]/45 to-black/75"
      ></div>
      <div
        class="absolute inset-0 bg-[radial-gradient(900px_circle_at_30%_20%,rgba(70,140,185,0.18),transparent_60%)]"
      ></div>

      <div class="absolute inset-0 flex items-center">
        <div class="px-14">
          <p class="text-white/85 text-3xl font-semibold leading-tight">
            Welcome back.
          </p>
          <p class="mt-2 text-white/60 text-lg">
            Reserve a sports facility that meets your needs in no time.
          </p>
        </div>
      </div>
    </div>
  </div>
</div>
