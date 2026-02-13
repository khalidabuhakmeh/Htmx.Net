using JetBrains.Annotations;

namespace Htmx;

/// <summary>
/// Standard HTMX Swap styles
/// https://htmx.org/attributes/hx-swap/
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
}
