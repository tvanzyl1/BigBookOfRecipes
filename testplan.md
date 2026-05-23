# BigBookOfRecipes Test Plan

## Purpose

This test plan verifies that BigBookOfRecipes works as a local Blazor recipe book with JSON persistence, recipe browsing, search, admin management, and cookbook-style presentation.

## Environment

- Windows 10 or later
- PowerShell 7
- .NET 10 SDK, or the highest available SDK if .NET 10 is not installed
- Browser: Edge, Chrome, or Firefox

## Build and run checks

### 1. Restore packages

```bash
dotnet restore
```

Expected result:

- Packages restore successfully.

### 2. Build

```bash
dotnet build
```

Expected result:

- Build succeeds with no errors.

### 3. Run

```bash
dotnet run
```

Expected result:

- App starts locally.
- Console displays the localhost URL.
- App opens successfully in a browser.

## Functional tests

### Home page

| Test | Steps | Expected result |
|---|---|---|
| Home loads | Open `/` | Home page loads without errors |
| Sections visible | View home page | Breakfast, Lunch, Dinner, Dessert, Drinks or similar sections are shown |
| Admin link visible | View home page | Link to admin area is available |
| Browse link visible | View home page | Link to browse all recipes is available |
| Search visible | View home page | Search box is available |

### Search

| Test | Steps | Expected result |
|---|---|---|
| Search by recipe name | Search for a known recipe name | Matching recipe appears |
| Search by ingredient | Search for an ingredient used in a seeded recipe | Matching recipe appears |
| Search by section | Search for Breakfast, Dinner, etc. | Recipes in matching section appear |
| Search partial word | Type part of a recipe name | Matching recipe appears |
| Search typo tolerance | Type a simple typo | Relevant result still appears, where fuzzy search can reasonably match |
| No result | Search for nonsense text | Friendly no-results message appears |

### Browse recipes

| Test | Steps | Expected result |
|---|---|---|
| Browse all | Open `/recipes` | All recipes are listed |
| Open recipe | Click a recipe | Recipe detail page opens |
| Section page | Click a section | Recipes for that section are listed |

### Recipe detail

| Test | Steps | Expected result |
|---|---|---|
| Name and description | Open a recipe | Name and description appear near the top |
| Ingredients | Open a recipe | Ingredients appear in the left panel on desktop |
| Optional measurement | Add or view ingredient without measurement | Ingredient displays cleanly |
| Markdown steps | Open recipe with Markdown | Markdown renders correctly |
| Responsive layout | Resize browser narrow | Ingredients stack above steps |

### Admin sections

| Test | Steps | Expected result |
|---|---|---|
| Open sections admin | Open `/admin/sections` | Section management page opens |
| Create section | Add a new section | Section appears on home page and admin list |
| Edit section | Rename section | New name appears and route slug updates |
| Delete section | Delete section after confirmation | Section is removed |
| Duplicate section | Add existing section name | Validation prevents duplicate or shows useful error |

### Admin recipes

| Test | Steps | Expected result |
|---|---|---|
| Open recipes admin | Open `/admin/recipes` | Recipe list appears |
| Create recipe | Add name, description, section, ingredient, and steps | Recipe is saved |
| Edit recipe | Change description or steps | Changes persist |
| Delete recipe | Delete recipe after confirmation | Recipe is removed |
| Multiple sections | Assign recipe to two sections | Recipe appears in both section pages |
| Ingredient without measurement | Save ingredient with only name | Recipe saves successfully |
| Missing name | Save recipe without name | Validation prevents save |
| Missing ingredients | Save recipe without ingredients | Validation prevents save |
| Missing steps | Save recipe without steps | Validation prevents save |

## Persistence tests

| Test | Steps | Expected result |
|---|---|---|
| JSON files created | Delete or rename Data JSON files, then run app | Files are recreated with seed data |
| Save persists | Create recipe, stop app, restart app | Recipe still exists |
| Edit persists | Edit recipe, stop app, restart app | Edited values remain |
| Delete persists | Delete recipe, stop app, restart app | Deleted recipe does not return |
| JSON readable | Open JSON files | Files are indented and human-readable |

## Styling tests

| Test | Steps | Expected result |
|---|---|---|
| Not Bootstrap default | View pages | App has custom cookbook styling |
| Recipe cards | View home/browse pages | Cards look polished and readable |
| Forms | View admin forms | Forms are clear and pleasant to use |
| Mobile | Use browser responsive mode | Layout remains usable |

## Accessibility checks

| Test | Steps | Expected result |
|---|---|---|
| Labels | Inspect admin forms | Inputs have visible labels |
| Keyboard | Navigate with keyboard | Main navigation and forms are usable |
| Focus | Tab through page | Focus indicator is visible |
| Contrast | Review text and controls | Text is readable against backgrounds |
| Semantic headings | Inspect page structure | Pages use sensible headings |

## Regression checklist

Before considering the implementation complete:

- `dotnet restore` succeeds.
- `dotnet build` succeeds.
- App runs locally.
- Home page works.
- Search works.
- Browse all recipes works.
- Recipe detail works.
- Markdown rendering works.
- Admin section CRUD works.
- Admin recipe CRUD works.
- JSON persistence works.
- README has run instructions.
- `IMPLEMENTATION_NOTES.md` documents assumptions and limitations.
