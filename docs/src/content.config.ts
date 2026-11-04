import { defineCollection, z } from "astro:content";
import { docsLoader } from "@astrojs/starlight/loaders";
import { docsSchema } from "@astrojs/starlight/schema";

// Site-wide seasonal banner. Applied as a schema default so every page shows it
// without per-page frontmatter. Remove this `banner` default after the holidays.
export const collections = {
  docs: defineCollection({
    loader: docsLoader(),
    schema: docsSchema({
      extend: z.object({
        banner: z
          .object({ content: z.string() })
          .default({
            content:
              '🎄 Happy holidays! Explore <a href="https://github.com/khalidabuhakmeh/twelve-days-of-htmx-mas">Twelve Days of htmx-mas</a> — 12 Christmas-themed htmx + ASP.NET Core demos. ⛄',
          }),
      }),
    }),
  }),
};
