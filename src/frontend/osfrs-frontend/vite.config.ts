import tailwindcss from "@tailwindcss/vite";
import { sveltekit } from "@sveltejs/kit/vite";
import { defineConfig } from "vite";

export default defineConfig({
    server: {
        host: "0.0.0.0",
        port: 7500,
        allowedHosts: ["spacecraft"],
    },
    plugins: [tailwindcss(), sveltekit()],
});
