<script lang="ts">
  import type {
    HTMLInputAttributes,
    HTMLInputTypeAttribute,
  } from "svelte/elements";
  import { cn, type WithElementRef } from "$lib/utils.js";
  type InputType = Exclude<HTMLInputTypeAttribute, "file">;
  type Props = WithElementRef<
    Omit<HTMLInputAttributes, "type"> &
      (
        | { type: "file"; files?: FileList }
        | { type?: InputType; files?: undefined }
      )
  >;
  let {
    ref = $bindable(null),
    value = $bindable(),
    type,
    files = $bindable(),
    class: className,
    "data-slot": dataSlot = "input",
    ...restProps
  }: Props = $props();
</script>

{#if type === "file"}
  <input
    bind:this={ref}
    data-slot={dataSlot}
    class={cn(
      "selection:bg-primary selection:text-text-primary placeholder:text-text-disabled flex h-9 min-w-0 px-3 py-1 text-base shadow-xs transition-[color,box-shadow,border] outline-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm",
      "focus-visible:border-text-primary focus-visible:border-2",
      "font-medium btn-neutral border-[#e6f0ff77] hover:border-[#e6f0ff99] text-md xs:text-lg w-full xs:w-50",
      className,
    )}
    type="file"
    bind:files
    bind:value
    {...restProps}
  />
{:else}
  <input
    bind:this={ref}
    data-slot={dataSlot}
    class={cn(
      "selection:bg-primary selection:text-text-primary placeholder:text-text-disabled flex h-9 min-w-0 px-3 py-1 text-base shadow-xs transition-[color,box-shadow,border] outline-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm",
      "focus-visible:border-text-primary focus-visible:border-2",
      "font-medium btn-neutral border-[#e6f0ff77] hover:border-[#e6f0ff99] text-md xs:text-lg w-full xs:w-50",
      className,
    )}
    {type}
    bind:value
    {...restProps}
  />
{/if}
