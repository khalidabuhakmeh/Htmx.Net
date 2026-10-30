// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";

// NOTE: `site` and `base` target GitHub Pages for khalidabuhakmeh/Htmx.Net.
// Task E5 finalizes these to match the actual Pages URL.
export default defineConfig({
  site: "https://khalidabuhakmeh.github.io",
  base: "/Htmx.Net",
  integrations: [
    starlight({
      title: "Htmx.Net",
      social: [
        { icon: "github", label: "GitHub", href: "https://github.com/khalidabuhakmeh/Htmx.Net" },
      ],
      sidebar: [
        { label: "Getting Started", items: [{ autogenerate: { directory: "getting-started" } }] },
        { label: "Guides", items: [{ autogenerate: { directory: "guides" } }] },
        { label: "Reference", items: [{ autogenerate: { directory: "reference" } }] },
        { label: "Samples", items: [{ autogenerate: { directory: "samples" } }] },
        { label: "Migration (v1 → v2)", items: [{ autogenerate: { directory: "migration" } }] },
        { label: "Previous Version (v1.x)", items: [{ autogenerate: { directory: "v1" } }] },
      ],
    }),
  ],
});
