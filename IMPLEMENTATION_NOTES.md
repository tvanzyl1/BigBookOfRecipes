# Implementation Notes

- Target framework: `net10.0` using the installed .NET 10 SDK.
- Persistence uses local JSON files in `Data/` with async reads, indented writes, and a semaphore around write operations.
- Markdown rendering is implemented with a minimal built-in renderer because package restore was blocked by a local NuGet lock during scaffolding. Supported syntax covers headings, ordered lists, unordered lists, bold, italics, inline code, and paragraphs.
- Automated verification lives in `BigBookOfRecipes.Tests` and covers slug generation, fuzzy search, Markdown rendering, and JSON repository CRUD.
- Browser automation via Playwright could not be used in this environment because the configured Chromium/Chrome runtime was unavailable, so route verification was completed through direct HTTP checks against the running app.
