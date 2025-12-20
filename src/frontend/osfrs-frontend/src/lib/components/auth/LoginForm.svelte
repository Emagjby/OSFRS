<script lang="ts">
  import {
    FieldGroup,
    Field,
    FieldLabel,
  } from "$lib/components/ui/field/index.js";
  import { Input } from "$lib/components/ui/input/index.js";
  import { href } from "$lib/nav";
  import { cn, type WithElementRef } from "$lib/utils.js";
  import type { HTMLFormAttributes } from "svelte/elements";
  import FieldSeparator from "../ui/field/FieldSeparator.svelte";
  let {
    ref = $bindable(null),
    class: className,
    ...restProps
  }: WithElementRef<HTMLFormAttributes> = $props();
  const id = $props.id();
</script>

<form
  class={cn("flex flex-col gap-6", className)}
  bind:this={ref}
  {...restProps}
>
  <FieldGroup>
    <div class="flex flex-col items-center gap-1 text-center">
      <h1 class="text-2xl font-bold">Sign in to your account</h1>
      <p class="text-muted-foreground text-sm text-balance">
        Use your email and password to continue.
      </p>
    </div>
    <Field>
      <FieldLabel for="email-{id}">Email</FieldLabel>
      <Input
        id="email-{id}"
        type="email"
        placeholder="email@example.com"
        required
      />
    </Field>
    <Field>
      <div class="flex items-center">
        <FieldLabel for="password-{id}">Password</FieldLabel>
        <a
          href={href("/auth/reset-password")}
          class="ms-auto text-sm text-white/60 hover:text-white/80 underline-offset-4 hover:underline"
        >
          Forgot your password?
        </a>
      </div>
      <Input id="password-{id}" type="password" required />
    </Field>
    <Field>
      <button
        class="btn-neutral w-full bg-[#f6f0ffe9] hover:bg-[#f6f0ff] font-semibold text-[#2f2f43] cursor-pointer"
        type="submit">Login</button
      >
    </Field>
  </FieldGroup>
</form>
