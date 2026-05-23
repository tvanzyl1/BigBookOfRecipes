# BigBookOfRecipes

BigBookOfRecipes is a local cookbook application built with .NET 10 and Blazor. It uses JSON files for persistence, provides public recipe browsing and search, and includes a lightweight admin area for managing sections and recipes without introducing a database.

The application is designed to feel like a practical recipe book rather than a default starter app: warm styling, section-led browsing, readable recipe pages, and straightforward editing flows.

## What the project does

- Shows recipe sections such as Breakfast, Lunch, Dinner, Dessert, and Drinks
- Supports recipe search across:
  - recipe name
  - description
  - section name
  - ingredients
  - step text
- Uses forgiving fuzzy matching for partial queries and simple typos
- Provides:
  - a home page
  - a browse-all recipes page
  - section pages
  - recipe detail pages
  - an admin dashboard
  - admin CRUD for sections
  - admin CRUD for recipes
- Stores data in:
  - `Data/sections.json`
  - `Data/recipes.json`
- Seeds the app with demo data if the JSON files are missing

## Main features

### Public experience

- Cookbook-style home page
- Search results ranked by relevance
- Browse all recipes
- Browse recipes by section
- Recipe detail page with:
  - description near the title
  - ingredients in a left-hand panel on larger screens
  - steps in a right-hand panel
  - responsive stacking on smaller screens
  - Markdown-rendered instructions

### Admin experience

- Manage recipe sections
- Create, edit, and delete recipes
- Assign recipes to one or more sections
- Add ingredients with optional measurements
- Write recipe steps in Markdown
- Preview rendered Markdown while editing
- Confirmation prompts before destructive actions

## Technical summary

- Framework: `ASP.NET Core Blazor Web App`
- Target framework: `net10.0`
- Persistence: `System.Text.Json` with local file storage
- Search: custom scoring with contains matching and lightweight typo tolerance
- Markdown: built-in safe renderer for headings, paragraphs, ordered lists, unordered lists, bold, italics, and inline code
- Tests: xUnit test project covering repository behavior and core services

## Repository layout

- `Components/Pages` - public pages and admin pages
- `Components/Layout` - app shell and shared layout
- `Models` - recipe, ingredient, and section models
- `Services` - repository, search, slug generation, and Markdown rendering
- `Data` - seeded JSON persistence files
- `BigBookOfRecipes.Tests` - automated tests
- `wwwroot` - custom styling and static assets

## Prerequisites

- Windows, macOS, or Linux with the .NET 10 SDK installed
- A modern browser

Check the installed SDKs with:

```powershell
dotnet --list-sdks
```

## Build the project

From the repository root:

```powershell
dotnet restore
dotnet build
```

Expected result:

- restore succeeds
- build succeeds with no errors

## Run the project

From the repository root:

```powershell
dotnet run --urls http://127.0.0.1:5015
```

Then open:

```text
http://127.0.0.1:5015
```

If you prefer the default launch profile, you can also run:

```powershell
dotnet run
```

## Test the project

Run the focused automated test suite:

```powershell
dotnet test BigBookOfRecipes.Tests\BigBookOfRecipes.Tests.csproj
```

The test project covers:

- slug uniqueness
- fuzzy search behavior
- Markdown rendering safety
- JSON repository seed, save, update, and delete behavior

## Data storage

Application data is stored in JSON files under `Data/`.

- `sections.json` stores recipe sections
- `recipes.json` stores recipes, ingredients, section links, and Markdown steps

Behavior:

- files are created if missing
- demo data is seeded automatically
- writes are protected with a semaphore
- JSON is written with indentation for manual inspection and editing

## Notes

- This version is intentionally local and file-based.
- It does not use SQL, EF Core, or authentication.
- The Markdown renderer is intentionally limited and avoids raw HTML rendering.
- Additional implementation details and environment notes are in `IMPLEMENTATION_NOTES.md`.
