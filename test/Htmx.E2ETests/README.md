# Htmx.E2ETests

Playwright tests, tagged `[Trait("Category","E2E")]`. Exclude with `dotnet test --filter "Category!=E2E"`.

One-time browser install after building:

    pwsh test/Htmx.E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium
