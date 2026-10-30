using System.Runtime.CompilerServices;
using DiffEngine;

namespace Htmx.Tests;

/// <summary>
/// Test-assembly initialization. Disables the DiffEngine diff-tool launcher so a failing
/// Verify snapshot never pops open an external compare window (e.g. the Rider compare tool).
/// Mismatches are still reported in test output and written as <c>*.received.*</c> files.
/// </summary>
internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // Suppress launching any diff/merge tool (Rider, VS, etc.) on snapshot mismatch.
        DiffRunner.Disabled = true;
    }
}
