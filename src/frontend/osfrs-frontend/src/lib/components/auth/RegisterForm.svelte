<script lang="ts">
  import {
    FieldGroup,
    Field,
    FieldLabel,
    FieldDescription,
    FieldSeparator,
  } from "$lib/components/ui/field/index.js";
  import { Input } from "$lib/components/ui/input/index.js";
  import { href } from "$lib/nav";
  import { cn, type WithElementRef } from "$lib/utils.js";
  import type { HTMLFormAttributes } from "svelte/elements";
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
      <h1 class="text-2xl font-bold">Create your account</h1>
      <p class="text-muted-foreground text-sm text-balance">
        Use your email and password to get started.
      </p>
    </div>
    <Field class="-mb-2">
      <FieldLabel for="name-{id}">Name</FieldLabel>
      <Input id="name-{id}" type="text" placeholder="John Doe" required />
    </Field>
    <Field class="-mb-2">
      <FieldLabel for="username-{id}">Username</FieldLabel>
      <Input id="username-{id}" type="text" placeholder="johndoe" required />
    </Field>
    <Field class="-mb-2">
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
      </div>
      <Input id="password-{id}" type="password" required />
    </Field>
    <Field>
      <button
        class="btn-neutral w-full bg-[#f6f0ffe9] hover:bg-[#f6f0ff] font-semibold text-[#2f2f43] cursor-pointer"
        type="submit">Register</button
      >
    </Field>
    <FieldSeparator />
    <FieldDescription class="text-center">
      Already have an account?
      <a href={href("/auth?mode=login")} class="underline underline-offset-4"
        >Sign in.</a
      >
    </FieldDescription>
  </FieldGroup>
</form>
