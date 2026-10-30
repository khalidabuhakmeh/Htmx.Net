---
title: Headers tag helper
description: Build the hx-headers JSON from hx-headers-* attributes, with htmx 4 explicit inheritance.
---

`HtmxHeadersTagHelper` collects every `hx-headers-<Name>` attribute into one `hx-headers` JSON attribute.

```cshtml
<button hx-get="/demo/echo" hx-headers-X-Demo="own-button" hx-headers-X-Other="1">Go</button>
```

renders

```html
<button hx-get="/demo/echo" hx-headers='{"X-Demo":"own-button","X-Other":"1"}'>Go</button>
```

- Existing `hx-headers` JSON (and `data-hx-headers`, `hx-headers:inherited`) is merged; prefixed attributes win on conflict. A non-JSON existing value is left untouched.
- Properties: `HeaderAttributes` (the dictionary of prefixed attributes) and `Inherited`.

## Inheritance: `hx-headers-inherited`

htmx 4 attributes are not inherited by default. Add `hx-headers-inherited="true"` to emit `hx-headers:inherited` so descendants send the headers:

```cshtml
<!-- samples/Htmx4.Sample/Pages/Inherited.cshtml -->
<section hx-target:inherited="#inh-out" hx-headers-X-Demo="from-parent" hx-headers-inherited="true">
    <button hx-get="/demo/inherited/echo?p=inh">child gets target and headers</button>
</section>
```

Without the flag, the child does not receive the parent's headers.
