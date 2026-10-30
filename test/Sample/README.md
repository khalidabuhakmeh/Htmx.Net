# Legacy sample (htmx 1.x / 2.x) — frozen

> **This sample is frozen and is no longer part of the solution build.**

It targets **htmx 1.9.2** and the **v1.x** line of `Htmx` / `Htmx.TagHelpers`. It does **not**
compile against the htmx 4 (v2.0+) libraries in this branch, which is why it was removed from
`Htmx.sln` as part of the htmx 4 upgrade.

## Where to look instead

- **New htmx 4 sample:** [`samples/Htmx4.Sample`](../../samples/Htmx4.Sample) — demonstrates every
  library feature against htmx 4.0.0, with integration and Playwright E2E tests.
- **v1.x source & this sample as it last worked:** checkout the **`v1-final`** git tag.

  ```console
  git checkout v1-final
  ```

- **v1.x documentation:** the "Previous Version (v1.x)" section of the documentation site
  (see `docs/`).

This directory is kept in the repository on the upgrade branch only for reference during the
migration and may be deleted in a future release.
