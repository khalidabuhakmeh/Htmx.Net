using Microsoft.AspNetCore.Cors.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Htmx;

public static class HtmxCorsPolicyBuilderExtensions
{
    /// <summary>
    /// Adds Htmx request headers to the policy.
    /// </summary>
    /// <param name="policyBuilder"></param>
    /// <param name="excludeHeaders"></param>
    /// <returns>The current policy builder.</returns>
    public static CorsPolicyBuilder WithHtmxHeaders(this CorsPolicyBuilder policyBuilder, params string[] excludeHeaders)
    {
        IEnumerable<string> headers = new List<string> ()
        {
            HtmxRequestHeaders.Keys.CurrentUrl,
            HtmxRequestHeaders.Keys.HistoryRestoreRequest,
            HtmxRequestHeaders.Keys.Prompt,
            HtmxRequestHeaders.Keys.Request,
            HtmxRequestHeaders.Keys.Target,
            HtmxRequestHeaders.Keys.TriggerName,
            HtmxRequestHeaders.Keys.Trigger,
            HtmxRequestHeaders.Keys.Boosted
        };

        if (excludeHeaders.Length > 0)
        {
            headers = headers.Except(excludeHeaders);
        }

        policyBuilder.WithHeaders(headers.ToArray());

        return policyBuilder;
    }

    /// <summary>
    /// Adds Htmx response headers to the policy.
    /// </summary>
    /// <returns>The current policy builder.</returns>
    public static CorsPolicyBuilder WithExposedHtmxHeaders(this CorsPolicyBuilder policyBuilder, params string[] excludeHeaders)
    {
        IEnumerable<string> headers = new List<string>()
        {
            HtmxResponseHeaders.Keys.PushUrl,
            HtmxResponseHeaders.Keys.Location,
            HtmxResponseHeaders.Keys.Redirect,
            HtmxResponseHeaders.Keys.Refresh,
            HtmxResponseHeaders.Keys.Trigger,
            HtmxResponseHeaders.Keys.TriggerAfterSettle,
            HtmxResponseHeaders.Keys.TriggerAfterSwap,
            HtmxResponseHeaders.Keys.Reswap,
            HtmxResponseHeaders.Keys.Retarget,
            HtmxResponseHeaders.Keys.ReplaceUrl
        };

        if (excludeHeaders.Length > 0)
        {
            headers = headers.Except(excludeHeaders);
        }

        policyBuilder.WithExposedHeaders(headers.ToArray());

        return policyBuilder;
    }
}
