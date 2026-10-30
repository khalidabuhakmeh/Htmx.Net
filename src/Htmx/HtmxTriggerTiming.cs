namespace Htmx;

/// <summary>
/// Defines when a trigger runs
/// https://htmx.org/headers/hx-trigger/
/// </summary>
[System.Obsolete("htmx 4 only supports the HX-Trigger response header. Trigger-after-swap/settle timings were removed in htmx 4.", error: true)]
public enum HtmxTriggerTiming
{
    /// <summary>
    /// Triggers event as soon as the response is received
    /// </summary>
    Default,

    /// <summary>
    /// Triggers event after the settling step
    /// https://htmx.org/docs/#request-operations
    /// </summary>
    AfterSettle,

    /// <summary>
    /// Triggers event after the swap step
    /// https://htmx.org/docs/#request-operations
    /// </summary>
    AfterSwap
}