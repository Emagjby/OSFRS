import { resolve } from "$app/paths";

export function href(path: string, params?: URLSearchParams) {
	// @ts-expect-error — resolve() typings are too strict for composition
	const base = resolve(path);
	return params ? `${base}?${params.toString()}` : base;
}
