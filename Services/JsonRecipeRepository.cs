using System.Text.Json;
using BigBookOfRecipes.Models;

namespace BigBookOfRecipes.Services;

public sealed class JsonRecipeRepository : IRecipeRepository
{
    private readonly SlugService _slugService;
    private readonly IWebHostEnvironment _environment;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true
    };

    private string DataDirectory => Path.Combine(_environment.ContentRootPath, "Data");
    private string SectionsPath => Path.Combine(DataDirectory, "sections.json");
    private string RecipesPath => Path.Combine(DataDirectory, "recipes.json");

    public JsonRecipeRepository(SlugService slugService, IWebHostEnvironment environment)
    {
        _slugService = slugService;
        _environment = environment;
    }

    public async Task<IReadOnlyList<RecipeSection>> GetSectionsAsync()
    {
        await EnsureSeedDataAsync();
        var sections = await ReadJsonAsync<List<RecipeSection>>(SectionsPath) ?? [];
        return sections
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<IReadOnlyList<Recipe>> GetRecipesAsync()
    {
        await EnsureSeedDataAsync();
        var recipes = await ReadJsonAsync<List<Recipe>>(RecipesPath) ?? [];
        return recipes
            .OrderBy(recipe => recipe.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<RecipeSection?> GetSectionBySlugAsync(string slug) =>
        (await GetSectionsAsync()).FirstOrDefault(section => string.Equals(section.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public async Task<Recipe?> GetRecipeBySlugAsync(string slug) =>
        (await GetRecipesAsync()).FirstOrDefault(recipe => string.Equals(recipe.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public async Task<Recipe?> GetRecipeByIdAsync(Guid id) =>
        (await GetRecipesAsync()).FirstOrDefault(recipe => recipe.Id == id);

    public async Task<RecipeSection> SaveSectionAsync(RecipeSection section)
    {
        await _writeLock.WaitAsync();
        try
        {
            await EnsureSeedDataAsync();
            var sections = await ReadJsonAsync<List<RecipeSection>>(SectionsPath) ?? [];
            var normalizedName = section.Name.Trim();
            if (sections.Any(existing => existing.Id != section.Id && string.Equals(existing.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A section with that name already exists.");
            }

            section.Name = normalizedName;
            section.Description = string.IsNullOrWhiteSpace(section.Description) ? null : section.Description.Trim();
            section.Slug = _slugService.EnsureUniqueSlug(section.Name, sections.Select(existing => existing.Slug), section.Slug);

            var existingIndex = sections.FindIndex(existing => existing.Id == section.Id);
            if (existingIndex >= 0)
            {
                sections[existingIndex] = section;
            }
            else
            {
                if (section.SortOrder == 0)
                {
                    section.SortOrder = sections.Count == 0 ? 10 : sections.Max(existing => existing.SortOrder) + 10;
                }

                sections.Add(section);
            }

            await WriteJsonAsync(SectionsPath, sections);
            return section;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> DeleteSectionAsync(Guid id)
    {
        await _writeLock.WaitAsync();
        try
        {
            await EnsureSeedDataAsync();
            var sections = await ReadJsonAsync<List<RecipeSection>>(SectionsPath) ?? [];
            var removed = sections.RemoveAll(section => section.Id == id) > 0;
            if (!removed)
            {
                return false;
            }

            var recipes = await ReadJsonAsync<List<Recipe>>(RecipesPath) ?? [];
            foreach (var recipe in recipes)
            {
                recipe.SectionIds = recipe.SectionIds.Where(sectionId => sectionId != id).ToList();
            }

            await WriteJsonAsync(SectionsPath, sections);
            await WriteJsonAsync(RecipesPath, recipes);
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<Recipe> SaveRecipeAsync(Recipe recipe)
    {
        await _writeLock.WaitAsync();
        try
        {
            await EnsureSeedDataAsync();
            var recipes = await ReadJsonAsync<List<Recipe>>(RecipesPath) ?? [];

            recipe.Name = recipe.Name.Trim();
            recipe.Description = recipe.Description.Trim();
            recipe.StepsMarkdown = recipe.StepsMarkdown.Trim();
            recipe.SectionIds = recipe.SectionIds.Distinct().ToList();
            recipe.Ingredients = recipe.Ingredients
                .Where(ingredient => !string.IsNullOrWhiteSpace(ingredient.Name))
                .Select(ingredient => new Ingredient
                {
                    Name = ingredient.Name.Trim(),
                    Measurement = string.IsNullOrWhiteSpace(ingredient.Measurement) ? null : ingredient.Measurement.Trim()
                })
                .ToList();

            recipe.Slug = _slugService.EnsureUniqueSlug(recipe.Name, recipes.Select(existing => existing.Slug), recipe.Slug);
            recipe.UpdatedAt = DateTimeOffset.UtcNow;

            var existingIndex = recipes.FindIndex(existing => existing.Id == recipe.Id);
            if (existingIndex >= 0)
            {
                recipe.CreatedAt = recipes[existingIndex].CreatedAt;
                recipes[existingIndex] = recipe;
            }
            else
            {
                recipe.CreatedAt = DateTimeOffset.UtcNow;
                recipes.Add(recipe);
            }

            await WriteJsonAsync(RecipesPath, recipes);
            return recipe;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> DeleteRecipeAsync(Guid id)
    {
        await _writeLock.WaitAsync();
        try
        {
            await EnsureSeedDataAsync();
            var recipes = await ReadJsonAsync<List<Recipe>>(RecipesPath) ?? [];
            var removed = recipes.RemoveAll(recipe => recipe.Id == id) > 0;
            if (!removed)
            {
                return false;
            }

            await WriteJsonAsync(RecipesPath, recipes);
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task EnsureSeedDataAsync()
    {
        Directory.CreateDirectory(DataDirectory);

        if (!File.Exists(SectionsPath) || !File.Exists(RecipesPath))
        {
            var seedSections = CreateSeedSections();
            var seedRecipes = CreateSeedRecipes(seedSections);

            if (!File.Exists(SectionsPath))
            {
                await WriteJsonAsync(SectionsPath, seedSections);
            }

            if (!File.Exists(RecipesPath))
            {
                await WriteJsonAsync(RecipesPath, seedRecipes);
            }
        }
    }

    private async Task<T?> ReadJsonAsync<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return await JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions);
    }

    private async Task WriteJsonAsync<T>(string path, T data)
    {
        await using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, data, _serializerOptions);
    }

    private List<RecipeSection> CreateSeedSections()
    {
        var names = new[]
        {
            ("Breakfast", "Easy starts for slow mornings.", 10),
            ("Lunch", "Reliable midday plates that travel well.", 20),
            ("Dinner", "Comforting mains for busy evenings.", 30),
            ("Dessert", "Sweet finishes and weekend baking.", 40),
            ("Drinks", "Warm and cold pours for the table.", 50)
        };

        return names
            .Select(item => new RecipeSection
            {
                Name = item.Item1,
                Description = item.Item2,
                SortOrder = item.Item3,
                Slug = _slugService.GenerateSlug(item.Item1)
            })
            .ToList();
    }

    private List<Recipe> CreateSeedRecipes(IReadOnlyList<RecipeSection> sections)
    {
        Guid SectionId(string slug) => sections.First(section => section.Slug == slug).Id;

        return
        [
            new Recipe
            {
                Name = "Lemon Ricotta Pancakes",
                Slug = "lemon-ricotta-pancakes",
                Description = "Tender pancakes with a bright citrus finish and soft ricotta crumb.",
                SectionIds = [SectionId("breakfast")],
                Ingredients =
                [
                    new() { Measurement = "1 1/2 cups", Name = "plain flour" },
                    new() { Measurement = "2 tbsp", Name = "sugar" },
                    new() { Measurement = "1 tsp", Name = "baking powder" },
                    new() { Measurement = "3/4 cup", Name = "milk" },
                    new() { Measurement = "3/4 cup", Name = "ricotta" },
                    new() { Measurement = "1", Name = "lemon, zested" }
                ],
                StepsMarkdown = """
                ## Method

                1. Whisk the dry ingredients together in a large bowl.
                2. Fold in the milk, ricotta, egg, and lemon zest until just combined.
                3. Rest the batter for 10 minutes.
                4. Cook small ladles in a buttered pan until golden on both sides.

                - Serve with honey or berries.
                - Add a pinch of salt if your ricotta is very mild.
                """
            },
            new Recipe
            {
                Name = "Roast Tomato Soup",
                Slug = "roast-tomato-soup",
                Description = "A pantry-friendly soup with caramelised tomatoes, garlic, and basil.",
                SectionIds = [SectionId("lunch"), SectionId("dinner")],
                Ingredients =
                [
                    new() { Measurement = "1 kg", Name = "ripe tomatoes" },
                    new() { Measurement = "1", Name = "brown onion" },
                    new() { Measurement = "4 cloves", Name = "garlic" },
                    new() { Measurement = "2 cups", Name = "vegetable stock" },
                    new() { Measurement = "1 handful", Name = "basil leaves" }
                ],
                StepsMarkdown = """
                ## Method

                1. Roast the tomatoes, onion, and garlic at 220C until softened and charred at the edges.
                2. Transfer to a pot with stock and simmer for 15 minutes.
                3. Blend until smooth, then stir through basil.

                **Finish** with olive oil and cracked pepper.
                """
            },
            new Recipe
            {
                Name = "Brown Butter Chocolate Cookies",
                Slug = "brown-butter-chocolate-cookies",
                Description = "Chewy cookies with toasted butter and dark chocolate pockets.",
                SectionIds = [SectionId("dessert")],
                Ingredients =
                [
                    new() { Measurement = "170 g", Name = "unsalted butter" },
                    new() { Measurement = "3/4 cup", Name = "brown sugar" },
                    new() { Measurement = "1/2 cup", Name = "caster sugar" },
                    new() { Measurement = "1", Name = "egg" },
                    new() { Measurement = "200 g", Name = "dark chocolate" }
                ],
                StepsMarkdown = """
                ## Method

                1. Brown the butter, then cool slightly.
                2. Beat with the sugars, then add egg and vanilla.
                3. Mix in flour, baking soda, and chopped chocolate.
                4. Chill the dough for 30 minutes.
                5. Bake until the edges are set and the middle is still soft.
                """
            },
            new Recipe
            {
                Name = "Sparkling Citrus Iced Tea",
                Slug = "sparkling-citrus-iced-tea",
                Description = "Black tea with orange, lemon, and sparkling water for warm days.",
                SectionIds = [SectionId("drinks")],
                Ingredients =
                [
                    new() { Measurement = "3 bags", Name = "black tea" },
                    new() { Measurement = "2 tbsp", Name = "honey" },
                    new() { Measurement = "1", Name = "orange" },
                    new() { Measurement = "1", Name = "lemon" },
                    new() { Measurement = "500 ml", Name = "sparkling water" }
                ],
                StepsMarkdown = """
                Brew the tea strong, sweeten while warm, and chill completely.

                1. Add sliced citrus to a jug.
                2. Pour over the tea and sparkling water.
                3. Serve over plenty of ice.
                """
            }
        ];
    }
}
