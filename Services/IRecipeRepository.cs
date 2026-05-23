using BigBookOfRecipes.Models;

namespace BigBookOfRecipes.Services;

public interface IRecipeRepository
{
    Task<IReadOnlyList<RecipeSection>> GetSectionsAsync();
    Task<IReadOnlyList<Recipe>> GetRecipesAsync();
    Task<RecipeSection?> GetSectionBySlugAsync(string slug);
    Task<Recipe?> GetRecipeBySlugAsync(string slug);
    Task<Recipe?> GetRecipeByIdAsync(Guid id);
    Task<RecipeSection> SaveSectionAsync(RecipeSection section);
    Task<bool> DeleteSectionAsync(Guid id);
    Task<Recipe> SaveRecipeAsync(Recipe recipe);
    Task<bool> DeleteRecipeAsync(Guid id);
}
