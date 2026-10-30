using JetBrains.Annotations;

namespace Htmx;

/// <summary>
/// Standard HTMX Swap styles
/// https://four.htmx.org/reference/attributes/hx-swap
/// </summary>
[PublicAPI]
public static class HtmxSwap
{
    /// <summary>
    /// Replace the inner html of the target element
    /// </summary>
    public const string InnerHtml = "innerHTML";
    
    /// <summary>
    /// Replace the entire target element with the response
    /// </summary>
    public const string OuterHtml = "outerHTML";
    
    /// <summary>
    /// Insert the response before the target element
    /// </summary>
    public const string BeforeBegin = "beforebegin";
    
    /// <summary>
    /// Insert the response before the first child of the target element
    /// </summary>
    public const string AfterBegin = "afterbegin";
    
    /// <summary>
    /// Insert the response after the last child of the target element
    /// </summary>
    public const string BeforeEnd = "beforeend";
    
    /// <summary>
    /// Insert the response after the target element
    /// </summary>
    public const string AfterEnd = "afterend";
    
    /// <summary>
    /// Deletes the target element regardless of the response
    /// </summary>
    public const string Delete = "delete";
    
    /// <summary>
    /// Does not swap the content
    /// </summary>
    public const string None = "none";

    /// <summary>
    /// Morphs the target's attributes, then replaces its children, retaining the target (htmx 4)
    /// </summary>
    public const string OuterSync = "outerSync";

    /// <summary>
    /// Morphs the target's inner content (htmx 4)
    /// </summary>
    public const string InnerMorph = "innerMorph";

    /// <summary>
    /// Morphs the target element itself (htmx 4)
    /// </summary>
    public const string OuterMorph = "outerMorph";

    /// <summary>
    /// Sets the target's text content, without parsing HTML (htmx 4)
    /// </summary>
    public const string TextContent = "textContent";

    /// <summary>
    /// Alias of <see cref="BeforeBegin"/> (htmx 4)
    /// </summary>
    public const string Before = "before";

    /// <summary>
    /// Alias of <see cref="AfterBegin"/> (htmx 4)
    /// </summary>
    public const string Prepend = "prepend";

    /// <summary>
    /// Alias of <see cref="BeforeEnd"/> (htmx 4)
    /// </summary>
    public const string Append = "append";

    /// <summary>
    /// Alias of <see cref="AfterEnd"/> (htmx 4)
    /// </summary>
    public const string After = "after";
}
