using BigBookOfRecipes.Models;
using BigBookOfRecipes.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace BigBookOfRecipes.Tests;

public sealed class RecipeServicesTests : IDisposable
{
    private readonly string _contentRoot;

    public RecipeServicesTests()
    {
        _contentRoot = Path.Combine(Path.GetTempPath(), "BigBookOfRecipes.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_contentRoot);
    }

    [Fact]
    public void SlugService_AppendsSuffix_ForDuplicateSlugs()
    {
        var slugService = new SlugService();

        var slug = slugService.EnsureUniqueSlug("Roast Tomato Soup", ["roast-tomato-soup", "roast-tomato-soup-2"]);

        Assert.Equal("roast-tomato-soup-3", slug);
    }

    [Fact]
    public void SearchService_FindsRecipes_ByIngredientAndTypo()
    {
        var breakfast = new RecipeSection { Id = Guid.NewGuid(), Name = "Breakfast", Slug = "breakfast" };
        IReadOnlyList<Recipe> recipes =
        [
            new Recipe
            {
                Name = "Lemon Ricotta Pancakes",
                Description = "Soft pancakes",
                SectionIds = [breakfast.Id],
                Ingredients = [new Ingredient { Name = "ricotta" }],
                StepsMarkdown = "Whisk and cook."
            }
        ];

        var searchService = new RecipeSearchService();
        var byIngredient = searchService.Search("ricotta", recipes, new Dictionary<Guid, RecipeSection> { [breakfast.Id] = breakfast });
        var byTypo = searchService.Search("pankakes", recipes, new Dictionary<Guid, RecipeSection> { [breakfast.Id] = breakfast });

        Assert.Single(byIngredient);
        Assert.Single(byTypo);
        Assert.Equal("Lemon Ricotta Pancakes", byTypo[0].Recipe.Name);
    }

    [Fact]
    public async Task JsonRepository_CreatesSeedData_AndPersistsCrudChanges()
    {
        var repository = CreateRepository();

        var seedSections = await repository.GetSectionsAsync();
        var seedRecipes = await repository.GetRecipesAsync();

        Assert.NotEmpty(seedSections);
        Assert.NotEmpty(seedRecipes);

        var section = await repository.SaveSectionAsync(new RecipeSection
        {
            Name = "Snacks",
            Description = "Quick bites",
            SortOrder = 60
        });

        var recipe = await repository.SaveRecipeAsync(new Recipe
        {
            Name = "Cheese Toastie",
            Description = "Fast stovetop sandwich.",
            SectionIds = [section.Id],
            Ingredients = [new Ingredient { Name = "bread" }, new Ingredient { Name = "cheddar", Measurement = "2 slices" }],
            StepsMarkdown = "1. Butter the bread.\n2. Toast until golden."
        });

        var reloadedRecipe = await repository.GetRecipeBySlugAsync("cheese-toastie");
        Assert.NotNull(reloadedRecipe);
        Assert.Equal(recipe.Id, reloadedRecipe.Id);

        recipe.Description = "Golden stovetop sandwich.";
        await repository.SaveRecipeAsync(recipe);

        var updatedRecipe = await repository.GetRecipeByIdAsync(recipe.Id);
        Assert.Equal("Golden stovetop sandwich.", updatedRecipe?.Description);

        var deletedRecipe = await repository.DeleteRecipeAsync(recipe.Id);
        var deletedSection = await repository.DeleteSectionAsync(section.Id);

        Assert.True(deletedRecipe);
        Assert.True(deletedSection);
        Assert.Null(await repository.GetRecipeByIdAsync(recipe.Id));
    }

    [Fact]
    public void MarkdownRenderer_RendersBasicMarkdown_Safely()
    {
        var renderer = new MarkdownRenderer();

        var html = renderer.ToHtml("## Method\n\n1. Stir.\n2. Serve.\n\n**Warm** and <script>alert('x')</script>");

        Assert.Contains("<h2>Method</h2>", html);
        Assert.Contains("<ol>", html);
        Assert.Contains("<strong>Warm</strong>", html);
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    private JsonRecipeRepository CreateRepository()
    {
        var environment = new TestWebHostEnvironment
        {
            ApplicationName = "BigBookOfRecipes.Tests",
            ContentRootPath = _contentRoot,
            ContentRootFileProvider = new PhysicalFileProvider(_contentRoot),
            WebRootPath = _contentRoot,
            WebRootFileProvider = new PhysicalFileProvider(_contentRoot),
            EnvironmentName = "Development"
        };

        return new JsonRecipeRepository(new SlugService(), environment);
    }

    public void Dispose()
    {
        if (Directory.Exists(_contentRoot))
        {
            Directory.Delete(_contentRoot, recursive: true);
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
