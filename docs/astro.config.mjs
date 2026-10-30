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
      title: "htmx.NET",
      customCss: ["./src/styles/htmx.css"],
      // Light code blocks on light theme, dark on dark — matches four.htmx.org.
      expressiveCode: { themes: ["github-light", "github-dark"] },
      head: [
        {
          tag: "link",
          attrs: { rel: "preconnect", href: "https://fonts.googleapis.com" },
        },
        {
          tag: "link",
          attrs: { rel: "preconnect", href: "https://fonts.gstatic.com", crossorigin: true },
        },
        {
          tag: "link",
          attrs: {
            rel: "stylesheet",
            href: "https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&family=JetBrains+Mono:wght@400;600&display=swap",
          },
        },
      ],
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
