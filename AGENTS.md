# BigBookOfRecipes — Codex Agent Pack

## Mission

Implement **BigBookOfRecipes**, a .NET 10 Blazor recipe book application backed by a JSON database.

The app should feel like a warm, practical recipe book, not a default Blazor Bootstrap starter site.

## Source README

```md
# BigBookOfRecipes
A book of recipes

## Infrastructure
A dotnet 10 blazor recipe book.
- Json database

## Simple functionality
- Home page
- Admin area
- Recipes

### Home page
The home page should list sections for Recipes. ie. Breakfast, Lunch, Dinner, Dessert, Drinks etc.
The home page should have links to an admin area
The home page should have a search function to search for a recipe. Maybe a bit fuzzy search.
The home page should have a way to browse any recipes.

### Admin Area
Area to add sections
Area to add recipes. Can link to section(s)
The recipe should have a name and a description
A recipe should have a list of ingredients including measurements, but measurements is not madatory.
A recipe should have steps. The steps can bewritten in markdown
You should be able to create, delete and edit recipes.

### Recipe page
The recipe should have a description at the top next to or under the name
The recipe page should have a left hand area with all the ingredients
The recipe page has a right hand section with the steps to implement. The markdown can be rendered properly

### Look and feel
The style should not be normal blazor bootstrap, but rather look like a recipe book for some feeling.
```

## Working assumptions

- Use .NET 10 Blazor. If the installed SDK does not support `net10.0`, target the highest installed .NET SDK and leave a note in `IMPLEMENTATION_NOTES.md`.
- Prefer a Blazor Web App project unless the existing repository already has a different Blazor structure.
- Use JSON files as the database. Do not introduce SQL or EF Core.
- Keep the first version simple, local, and file-based.
- Build a polished vertical slice rather than over-engineering.

## Core user stories

### Home page

As a user, I can:

- See recipe sections such as Breakfast, Lunch, Dinner, Dessert, Drinks.
- Search recipes by name, description, section, ingredient, or step text.
- Use forgiving/fuzzy matching so minor typos still return useful results.
- Browse all recipes.
- Navigate to the admin area.

### Admin area

As an admin user, I can:

- Add, edit, and delete sections.
- Add, edit, and delete recipes.
- Assign a recipe to one or more sections.
- Add a recipe name and description.
- Add ingredients, where each ingredient has:
  - Name
  - Optional measurement
- Add recipe steps written in Markdown.
- Preview rendered Markdown where practical.

Authentication is not required for v1 unless an auth system already exists.

### Recipe page

As a user, I can:

- Open a recipe detail page.
- See the recipe name and description near the top.
- See ingredients in a left-hand panel.
- See steps in a right-hand panel.
- See Markdown-rendered instructions.

On smaller screens, stack the ingredients above the steps.

## Suggested data model

Create simple model classes similar to the following:

```csharp
public sealed class RecipeSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Slug { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class Ingredient
{
    public string Name { get; set; } = "";
    public string? Measurement { get; set; }
}

public sealed class Recipe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Description { get; set; } = "";
    public List<Guid> SectionIds { get; set; } = new();
    public List<Ingredient> Ingredients { get; set; } = new();
    public string StepsMarkdown { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
```

## JSON database

Use a folder such as:

```text
Data/
  recipes.json
  sections.json
```

Seed with a few useful demo recipes and default sections.

The JSON repository should:

- Create files if missing.
- Read and write asynchronously.
- Use `System.Text.Json`.
- Use indented JSON for easy manual editing.
- Use a lock/semaphore around writes to avoid file corruption.
- Regenerate slugs when names change.
- Avoid duplicate slugs by appending `-2`, `-3`, etc.

## Suggested services

Create services similar to:

```text
Services/
  IRecipeRepository.cs
  JsonRecipeRepository.cs
  SlugService.cs
  RecipeSearchService.cs
```

`RecipeSearchService` should support:

- Case-insensitive contains matching.
- Basic fuzzy scoring.
- Matching across recipe name, description, section names, ingredients, and steps.
- Returning best matches first.

Keep fuzzy search simple. A lightweight Levenshtein distance or token scoring approach is enough.

## Markdown rendering

Use a suitable Markdown library such as `Markdig` if available.

If adding a package is allowed, use:

```bash
dotnet add package Markdig
```

Render recipe steps safely. Avoid rendering untrusted raw HTML unless explicitly sanitised. For v1, configure Markdown rendering without enabling raw HTML extras.

## Suggested pages/routes

```text
/
  Home page with sections, search, browse all recipes

/recipes
  Browse all recipes

/recipes/{slug}
  Recipe detail page

/sections/{slug}
  Recipes in one section

/admin
  Admin dashboard

/admin/sections
  Manage sections

/admin/recipes
  Manage recipes

/admin/recipes/new
  Create recipe

/admin/recipes/{id:guid}/edit
  Edit recipe
```

## Look and feel requirements

Avoid default Bootstrap styling. Create a custom recipe-book feel.

Suggested visual direction:

- Warm paper-like background.
- Recipe cards with rounded corners.
- Serif headings for a cookbook feel.
- Soft shadows.
- Muted colours such as cream, warm brown, sage, tomato, and parchment.
- Ingredient panel styled like a handwritten shopping list or side note.
- Steps panel styled like a recipe page.
- Responsive layout.
- Clear hover and focus states.
- Keep the UI clean and readable.

Do not rely on external images. CSS gradients, borders, icons, and layout are enough.

## Accessibility

- Use semantic headings.
- Ensure all form inputs have labels.
- Ensure colour contrast is readable.
- Use buttons for actions, links for navigation.
- Add confirmation before deleting recipes or sections.
- Keep keyboard navigation usable.

## Validation rules

### Recipe

- Name is required.
- Description is recommended but can be short.
- At least one ingredient is required.
- At least one step is required.
- At least one section should be selected, unless the app supports “Uncategorised”.

### Section

- Name is required.
- Section names should be unique ignoring case.

## Implementation approach for Codex

Work in small commits/steps:

1. Inspect the current repository.
2. Create or update the Blazor project.
3. Add models.
4. Add JSON repository and seed data.
5. Add search service.
6. Add Markdown rendering.
7. Build home, browse, section, and recipe detail pages.
8. Build admin section management.
9. Build admin recipe management.
10. Replace default styling with cookbook styling.
11. Run formatting, build, and tests.
12. Update README with run instructions and implementation notes.

## Coding style

- Keep code simple and readable.
- Prefer small components over very large pages.
- Use nullable reference types correctly.
- Use `System.Text.Json`.
- Avoid unnecessary dependencies.
- Do not introduce a real database.
- Do not add authentication unless requested.
- Avoid large generated files.
- Keep demo data small and useful.

## Definition of done

The work is complete when:

- The app builds.
- The app runs locally.
- Home page lists sections.
- User can search recipes.
- User can browse all recipes.
- User can open recipe details.
- Recipe steps render Markdown.
- Admin user can add, edit, and delete sections.
- Admin user can add, edit, and delete recipes.
- Recipes can link to one or more sections.
- Data persists to JSON files.
- The UI no longer looks like default Blazor Bootstrap.
- README includes run instructions.
