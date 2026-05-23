# Codex Task — Build BigBookOfRecipes

You are working in the root of the `BigBookOfRecipes` repository.

Implement a .NET 10 Blazor recipe book application using a JSON database.

## Target outcome

A local Blazor app where users can browse, search, and read recipes, and use a simple admin area to manage sections and recipes.

## Required features

### 1. Home page

Build a home page that includes:

- App title and short introduction.
- Recipe section cards, for example:
  - Breakfast
  - Lunch
  - Dinner
  - Dessert
  - Drinks
- Search box for recipes.
- Search results with recipe name, description, and matching sections.
- Link to browse all recipes.
- Link to admin area.

Search should be forgiving enough to handle partial words and simple typos.

### 2. Browse and section pages

Build:

- `/recipes` page listing all recipes.
- `/sections/{slug}` page listing recipes for one section.

### 3. Recipe detail page

Build `/recipes/{slug}`.

The page must show:

- Recipe name.
- Recipe description.
- Ingredients in a left-hand panel.
- Steps in a right-hand panel.
- Steps rendered from Markdown.

On mobile, ingredients should stack above steps.

### 4. Admin area

Build a simple admin area. No authentication is required for v1.

Admin routes should include:

- `/admin`
- `/admin/sections`
- `/admin/recipes`
- `/admin/recipes/new`
- `/admin/recipes/{id:guid}/edit`

The admin area must support:

- Create, edit, and delete sections.
- Create, edit, and delete recipes.
- Assign recipes to one or more sections.
- Add/edit/remove ingredients.
- Ingredient measurement is optional.
- Recipe steps are written in Markdown.
- Deleting should ask for confirmation.

### 5. JSON database

Use JSON files for persistence.

Suggested layout:

```text
Data/
  recipes.json
  sections.json
```

Requirements:

- Create the JSON files if missing.
- Seed the app with default sections and a few demo recipes.
- Persist changes.
- Use `System.Text.Json`.
- Use indented JSON.
- Protect writes with a semaphore or equivalent.
- Do not use SQL or EF Core.

### 6. Styling

Replace default Blazor Bootstrap look and feel with custom recipe-book styling.

Design direction:

- Warm, cookbook-style UI.
- Paper or parchment background.
- Recipe cards.
- Serif-style headings.
- Soft shadows.
- Friendly, readable forms.
- Good desktop and mobile layouts.

Avoid external image dependencies.

## Suggested implementation details

### Models

Create:

- `RecipeSection`
- `Ingredient`
- `Recipe`

### Services

Create:

- `IRecipeRepository`
- `JsonRecipeRepository`
- `SlugService`
- `RecipeSearchService`

### Markdown

Use `Markdig` if package installation works:

```bash
dotnet add package Markdig
```

If package installation is blocked, implement a minimal Markdown renderer and document the limitation in `IMPLEMENTATION_NOTES.md`.

## Important constraints

- Keep it simple.
- Do not add login/authentication.
- Do not add a SQL database.
- Do not over-engineer.
- Prefer clean, maintainable code.
- Preserve any existing useful repository files.
- Update README with clear run instructions.

## Verification commands

Run the best available commands for this repository, for example:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run
```

If `dotnet test` is not applicable because there are no tests, say so in `IMPLEMENTATION_NOTES.md`.

## Deliverables

At the end, the repository should include:

- Working Blazor app.
- JSON data persistence.
- Admin CRUD.
- Search.
- Markdown-rendered recipe pages.
- Custom cookbook styling.
- Updated README.
- `IMPLEMENTATION_NOTES.md` with any assumptions, limitations, or SDK differences.
