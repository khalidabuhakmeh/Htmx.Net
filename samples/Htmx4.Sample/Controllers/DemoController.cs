using Htmx;
using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Htmx4.Sample.Controllers;

/// <summary>MVC-routed htmx fragments used by the demo pages. Every fragment carries data-testid hooks.</summary>
[Route("demo")]
public class DemoController : Controller
{
    private static int morphCounter;

    internal static string H(string? v) => WebUtility.HtmlEncode(v ?? string.Empty);

    /// <summary>
    /// Sample-only shortcut for returning an inline HTML fragment with an optional status code.
    /// It is NOT part of htmx.NET. In a real app you would usually return a partial view
    /// (<c>return Partial("_View", model);</c>, an MVC/Razor Pages <c>PartialViewResult</c>) or
    /// a Razor component, rather than building HTML strings by hand. <c>Content(html, "text/html")</c>
    /// is the direct equivalent when you already have the HTML.
    /// </summary>
    internal static ContentResult Frag(string html, int status = 200) =>
        new() { Content = html, ContentType = "text/html; charset=utf-8", StatusCode = status };

    private static string Row(string prefix, string name, string? value) =>
        $"<li>{name}: <code data-testid=\"{prefix}-{name}\">{H(value)}</code></li>";

    // ---------- 1. RequestHeaders ----------
    [HttpGet("request-headers")]
    public IActionResult RequestHeadersEcho(string p = "rh")
    {
        var isHtmx = Request.IsHtmx(out var h);
        var html = $"<div id=\"{p}-fragment\" data-testid=\"{p}-fragment\"><ul>"
                   + Row(p, "is-htmx", isHtmx.ToString())
                   + Row(p, "source", h?.Source)
                   + Row(p, "source-tag", h?.SourceElement?.TagName)
                   + Row(p, "source-id", h?.SourceElement?.Id)
                   + Row(p, "source-decoded-id", h?.SourceElement?.DecodedId)
                   + Row(p, "source-raw", h?.SourceElement?.Raw)
                   + Row(p, "target", h?.Target)
                   + Row(p, "target-tag", h?.TargetElement?.TagName)
                   + Row(p, "target-id", h?.TargetElement?.Id)
                   + Row(p, "request-type", h?.RequestType.ToString())
                   + Row(p, "is-partial", Request.IsHtmxPartial().ToString())
                   + Row(p, "is-full", Request.IsHtmxFullRequest().ToString())
                   + Row(p, "boosted", h?.Boosted.ToString())
                   + Row(p, "is-boosted", Request.IsHtmxBoosted().ToString())
                   + Row(p, "is-nonboosted", Request.IsHtmxNonBoosted().ToString())
                   + Row(p, "current-url", h?.CurrentUrl)
                   + Row(p, "history-restore", h?.HistoryRestoreRequest.ToString())
                   + Row(p, "is-history-restore", Request.IsHtmxHistoryRestoreRequest().ToString())
                   + Row(p, "prompt", h?.Prompt)
                   + "</ul></div>";
        return Frag(html);
    }

    // ---------- 2. ResponseHeaders ----------
    [HttpGet("resp/{name}")]
    public IActionResult Resp(string name)
    {
        var body = $"<span data-testid=\"resp-result\" data-header=\"{H(name)}\">{H(name)} applied</span>";
        switch (name)
        {
            case "push-url": Response.Htmx(h => h.PushUrl("/ResponseHeaders?pushed=1").WithVary()); break;
            case "prevent-push": Response.Htmx(h => h.PreventPush().WithVary("HX-Source", "HX-Target")); break;
            case "replace-url": Response.Htmx(h => h.ReplaceUrl("/ResponseHeaders?replaced=1")); break;
            case "prevent-replace": Response.Htmx(h => h.PreventReplace()); break;
            case "redirect": Response.Htmx(h => h.Redirect("/Boosted")); break;
            case "refresh": Response.Htmx(h => h.Refresh()); break;
            case "retarget":
                Response.Htmx(h => h.Retarget("#resp-retarget-target"));
                body = "<span data-testid=\"resp-retargeted\">retargeted content</span>";
                break;
            case "reswap":
                Response.Htmx(h => h.Reswap(HtmxSwap.BeforeEnd));
                body = "<span data-testid=\"resp-reswapped\">appended</span>";
                break;
            case "reselect":
                Response.Htmx(h => h.Reselect("#resp-pick"));
                body = "<div id=\"resp-skip\" data-testid=\"resp-skipped\">NOT selected</div>"
                       + "<div id=\"resp-pick\" data-testid=\"resp-picked\">selected by HX-Reselect</div>";
                break;
            case "location-string": Response.Htmx(h => h.Location("/Boosted")); break;
            case "location-object":
                Response.Htmx(h => h.Location(new HtmxLocation { Path = "/Location?handler=Landing", Target = "#resp-result", Swap = HtmxSwap.InnerHtml, Select = "#loc-incoming" }));
                break;
            default: return NotFound();
        }

        return Frag(body);
    }

    [HttpGet("swap/{style}")]
    public IActionResult SwapStyle(string style)
    {
        string? swap = style switch
        {
            "innerHTML" => HtmxSwap.InnerHtml,
            "outerHTML" => HtmxSwap.OuterHtml,
            "beforebegin" => HtmxSwap.BeforeBegin,
            "afterbegin" => HtmxSwap.AfterBegin,
            "beforeend" => HtmxSwap.BeforeEnd,
            "afterend" => HtmxSwap.AfterEnd,
            "delete" => HtmxSwap.Delete,
            "none" => HtmxSwap.None,
            "outerSync" => HtmxSwap.OuterSync,
            "innerMorph" => HtmxSwap.InnerMorph,
            "outerMorph" => HtmxSwap.OuterMorph,
            "textContent" => HtmxSwap.TextContent,
            "before" => HtmxSwap.Before,
            "prepend" => HtmxSwap.Prepend,
            "append" => HtmxSwap.Append,
            "after" => HtmxSwap.After,
            _ => null
        };
        if (swap is null) return NotFound();
        Response.Htmx(h => h.Reswap(swap));
        var inner = $"<span data-testid=\"swap-result-{style}\">{H(style)}</span>";
        // outerHTML/outerSync/outerMorph replace the target itself, so give the replacement the same id.
        return Frag(style is "outerHTML" or "outerSync" or "outerMorph"
            ? $"<div id=\"swap-{style}\" data-testid=\"swap-target-{style}\">{inner}</div>"
            : inner);
    }

    // ---------- 3. Triggers ----------
    [HttpGet("trigger/{kind}")]
    public IActionResult Trigger(string kind)
    {
        switch (kind)
        {
            case "single": Response.Htmx(h => h.WithTrigger("demo-single")); break;
            case "detail": Response.Htmx(h => h.WithTrigger("demo-detail", new { id = 42, message = "hello" })); break;
            case "scalar": Response.Htmx(h => h.WithTrigger("demo-scalar", "plain-value")); break;
            case "multi":
                Response.Htmx(h => h.WithTrigger("demo-multi-a").WithTrigger("demo-multi-b", new { n = 2 }));
                break;
            case "merge-names":
                Response.Headers["HX-Trigger"] = "demo-pre-a, demo-pre-b";
                Response.Htmx(h => h.WithTrigger("demo-added", new { n = 1 }));
                break;
            case "merge-json":
                Response.Headers["HX-Trigger"] = "{\"demo-pre-json\":{\"id\":7}}";
                Response.Htmx(h => h.WithTrigger("demo-added", new { n = 1 }));
                break;
            case "first-wins":
                Response.Htmx(h => h.WithTrigger("demo-dup", new { v = "first" }).WithTrigger("demo-dup", new { v = "second" }));
                break;
            default: return NotFound();
        }

        return Frag($"<span data-testid=\"trigger-result\">{H(kind)} sent: {H(Response.Headers["HX-Trigger"].ToString())}</span>"
                    + $"<code data-testid=\"trigger-header\" hidden>{H(Response.Headers["HX-Trigger"].ToString())}</code>");
    }

    // ---------- 4. Config ----------
    [HttpGet("config/swap")]
    public IActionResult ConfigSwap() =>
        Frag($"<li data-testid=\"cfg-swap-item\">default swap item {DateTime.UtcNow.Ticks}</li>");

    // ---------- 5. UrlHelper (controller / route targets) ----------
    [HttpGet("echo/{id?}", Name = "demo-echo")]
    public IActionResult Echo(string? id, string? p) =>
        Frag($"<span data-testid=\"url-result\">demo-echo id={H(id)} p={H(p)} path={H(Request.Path)}{H(Request.QueryString.Value)}</span>");

    // ---------- 6. Inherited ----------
    [HttpGet("inherited/echo")]
    public IActionResult InheritedEcho(string p = "inh")
    {
        Request.IsHtmx(out var h);
        var header = Request.Headers["X-Demo"].ToString();
        return Frag($"<span data-testid=\"{p}-header\">{(header.Length == 0 ? "none" : H(header))}</span>"
                    + $"<span data-testid=\"{p}-target\">{H(h?.TargetElement?.TagName)}#{H(h?.TargetElement?.Id)}</span>");
    }

    // ---------- 8. Boosted POST helper lives in AntiforgeryDemoController ----------

    // ---------- 9. Partials ----------
    [HttpGet("partials")]
    public IActionResult Partials() => Frag(
        "<hx-partial hx-target=\"#part-a\" hx-swap=\"innerHTML\"><span data-testid=\"part-a-content\">partial A (innerHTML)</span></hx-partial>"
        + "<hx-partial hx-target=\"#part-b\" hx-swap=\"beforeend\"><li data-testid=\"part-b-item\">partial B item</li></hx-partial>"
        + "<hx-partial hx-target=\"#part-nested\" hx-swap=\"innerHTML\"><span data-testid=\"part-nested-content\">partial into nested target (only exists after main swap)</span></hx-partial>"
        + "<div id=\"part-oob\" hx-swap-oob=\"true\" data-testid=\"part-oob\">OOB updated</div>"
        + "<div id=\"part-main-content\" data-testid=\"part-main-content\">main content <span id=\"part-nested\" data-testid=\"part-nested\">nested placeholder</span></div>");

    // ---------- 10. Morph ----------
    [HttpGet("morph/{kind}")]
    public IActionResult Morph(string kind)
    {
        var n = Interlocked.Increment(ref morphCounter);
        var id = kind switch { "plain" => "morph-plain-box", "outer" => "morph-outer-box", "inner" => "morph-inner-box", _ => null };
        if (id is null) return NotFound();
        var inputs = $"<input id=\"{kind}-input\" name=\"x\" data-testid=\"morph-{kind}-input\" /> <span data-testid=\"morph-{kind}-count\">{n}</span>";
        switch (kind)
        {
            case "outer":
                Response.Htmx(h => h.Reswap(HtmxSwap.OuterMorph));
                return Frag($"<div id=\"{id}\" data-testid=\"{id}\">{inputs}</div>");
            case "inner":
                Response.Htmx(h => h.Reswap(HtmxSwap.InnerMorph));
                return Frag(inputs);
            default:
                return Frag($"<div id=\"{id}\" data-testid=\"{id}\">{inputs}</div>");
        }
    }

    // ---------- 11. ErrorSwaps ----------
    [HttpGet("status/{code:int}")]
    public IActionResult Status(int code) => code switch
    {
        422 => Frag("<div data-testid=\"err-422-body\" class=\"error\">Validation failed: name is required</div>", 422),
        500 => Frag("<div data-testid=\"err-500-body\" class=\"error\">Server exploded</div>", 500),
        204 => NoContent(),
        304 => StatusCode(304),
        _ => NotFound()
    };

    // ---------- 13. Polling ----------
    [HttpGet("poll")]
    public IActionResult Poll(int n = 0, int stop = 3, string mode = "load")
    {
        n++;
        if (n >= stop)
            return Frag($"<div id=\"poll-{mode}\" data-testid=\"poll-{mode}-box\"><span data-testid=\"poll-{mode}-count\">{n}</span> <span data-testid=\"poll-{mode}-done\">stopped (no poller in response)</span></div>");

        var trigger = mode == "every" ? "every 1s" : "load delay:1s";
        return Frag($"<div id=\"poll-{mode}\" data-testid=\"poll-{mode}-box\" hx-get=\"/demo/poll?n={n}&stop={stop}&mode={mode}\" hx-trigger=\"{trigger}\" hx-swap=\"outerHTML\">"
                    + $"<span data-testid=\"poll-{mode}-count\">{n}</span> <span data-testid=\"poll-{mode}-running\">polling</span></div>");
    }
}

/// <summary>Antiforgery endpoints: [AutoValidateAntiforgeryToken] for unsafe verbs (strict) and manual validation (check).</summary>
[Route("demo/af")]
[AutoValidateAntiforgeryToken]
public class AntiforgeryDemoController(IAntiforgery antiforgery) : Controller
{
    // Reaching the action means the token validated; failures produce a 400 with an empty body.
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", Route = "strict")]
    public IActionResult Strict() =>
        DemoController.Frag($"<span data-testid=\"af-strict-result\">strict {Request.Method} validated by [AutoValidateAntiforgeryToken]</span>");

    [HttpPost("validate-attr")]
    [ValidateAntiForgeryToken]
    public IActionResult ValidateAttr() =>
        DemoController.Frag("<span data-testid=\"af-attr-result\">POST validated by [ValidateAntiForgeryToken]</span>");

    // Manual validation: echoes whether validation passed and where the token came from.
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", Route = "check")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Check()
    {
        var tokens = antiforgery.GetTokens(HttpContext);
        var hasHeader = Request.Headers.ContainsKey(tokens.HeaderName ?? "RequestVerificationToken");
        var hasForm = Request.HasFormContentType && Request.Form.ContainsKey(tokens.FormFieldName);
        var source = hasHeader ? "header" : hasForm ? "form" : "none";
        bool valid;
        try { valid = await antiforgery.IsRequestValidAsync(HttpContext); }
        catch (AntiforgeryValidationException) { valid = false; }

        var note = Request.Query.ContainsKey("note") ? Request.Query["note"].ToString()
            : Request.HasFormContentType && Request.Form.ContainsKey("note") ? Request.Form["note"].ToString() : "";
        var noteWhere = Request.Query.ContainsKey("note") ? "query" : note.Length > 0 ? "body" : "none";
        // Safe verbs are not validated by ASP.NET Core; report that explicitly.
        var safe = HttpMethods.IsGet(Request.Method);
        return DemoController.Frag(
            "<ul data-testid=\"af-result\">"
            + $"<li>method: <code data-testid=\"af-method\">{Request.Method}</code></li>"
            + $"<li>token-source: <code data-testid=\"af-token-source\">{source}</code></li>"
            + $"<li>valid: <code data-testid=\"af-valid\">{(safe ? "n/a" : valid.ToString())}</code></li>"
            + $"<li>note: <code data-testid=\"af-note\">{DemoController.H(note)}</code> (<code data-testid=\"af-note-where\">{noteWhere}</code>)</li>"
            + "</ul>", safe || valid ? 200 : 400);
    }
}
