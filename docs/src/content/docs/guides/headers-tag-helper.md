---
title: Headers tag helper
description: Build the hx-headers JSON from hx-headers-* attributes with HtmxHeadersTagHelper, merge existing JSON, and use htmx 4 explicit inheritance with hx-headers-inherited.
---

`hx-headers` expects a JSON string, which is awkward to write in Razor (quotes, escaping, computed values). `HtmxHeadersTagHelper` lets you write one attribute per header, `hx-headers-<Name>`, and emits a single valid `hx-headers` JSON attribute. Use it to send custom request headers such as a tenant id, an API version or a correlation id with htmx requests.

```razor
<button hx-get="/demo/echo" hx-headers-X-Demo="own-button" hx-headers-X-Other="1">Go</button>
```

renders

```html
<button hx-get="/demo/echo" hx-headers='{"X-Demo":"own-button","X-Other":"1"}'>Go</button>
```

Values can be Razor expressions:

```razor
<form hx-post="/orders" hx-headers-X-Tenant="@Model.TenantId" hx-headers-X-Request-Id="@Guid.NewGuid()">
    ...
</form>
```

On the server, read them like any header:

```csharp
var tenant = Request.Headers["X-Tenant"].ToString();
```

## Merging with existing `hx-headers`

- An existing `hx-headers` JSON object (also `data-hx-headers`, `hx-headers:inherited`, `data-hx-headers:inherited`) is merged with the prefixed attributes; **prefixed attributes win on conflict**.
- A non-JSON existing value is left untouched.

```razor
<div hx-headers='{"X-A":"1","X-B":"old"}' hx-headers-X-B="new" hx-get="/x"></div>
@* -> hx-headers='{"X-A":"1","X-B":"new"}' *@
```

## Members

| Member | Meaning |
|---|---|
| `HeaderAttributes` (`IDictionary<string, string?>`) | Collects every `hx-headers-*` attribute (the prefix is stripped). |
| `Inherited` (`bool`, attribute `hx-headers-inherited`) | When true, emit `hx-headers:inherited` instead of `hx-headers`. |
| `Process(...)` | Builds the JSON and writes the final attribute. |

## Inheritance: `hx-headers-inherited`

htmx 4 attributes are **not** inherited by default (`implicitInheritance` is `false`). A plain `hx-headers` on a parent is not sent by its children. Add `hx-headers-inherited="true"` to emit `hx-headers:inherited` so descendants send the headers:

```razor
<!-- samples/Htmx4.Sample/Pages/Inherited.cshtml -->
<section hx-target:inherited="#inh-out" hx-headers-X-Demo="from-parent" hx-headers-inherited="true">
    <button hx-get="/demo/inherited/echo?p=inh">child gets target and headers</button>
</section>

<!-- no :inherited, so the child does NOT inherit in htmx 4 -->
<section hx-target="#inh-out2" hx-headers='{"X-Demo":"from-parent-plain"}'>
    <button hx-get="/demo/inherited/echo?p=noinh">child gets nothing</button>
</section>
```

The server side of that demo reports what actually arrived:

```csharp
[HttpGet("inherited/echo")]
public IActionResult InheritedEcho(string p = "inh")
{
    Request.IsHtmx(out var h);
    var header = Request.Headers["X-Demo"].ToString();   // "from-parent" or empty
    var target = h?.TargetElement;                       // div#inh-out, or the button itself
    // ...
}
```

:::caution
Inheritance applies to the **attributes** that support it (`hx-headers`, `hx-target`, ...). The verbs `hx-get`, `hx-post`, ... are different: only a direct verb attribute makes an element send a request, and `hx-get:inherited` does not make descendants request. See the [URL tag helper](/Htmx.Net/guides/url-tag-helper/#inheritance-in-htmx-4).
:::

---

See the live demo: run `dotnet run --project samples/Htmx4.Sample` and open `/Inherited` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Inherited.cshtml)).

Related: [Request detection](/Htmx.Net/guides/request-detection/) (`TargetElement`), [Config tag helper](/Htmx.Net/guides/config/) (`implicitInheritance`).
