namespace BigBookOfRecipes.Models;

public sealed class RecipeSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
