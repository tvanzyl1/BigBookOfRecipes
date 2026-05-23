namespace BigBookOfRecipes.Models;

public sealed class SearchResult
{
    public required Recipe Recipe { get; init; }
    public required IReadOnlyList<RecipeSection> Sections { get; init; }
    public required double Score { get; init; }
}
