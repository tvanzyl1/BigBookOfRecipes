using BigBookOfRecipes.Models;

namespace BigBookOfRecipes.Services;

public sealed class RecipeSearchService
{
    public IReadOnlyList<SearchResult> Search(
        string? searchText,
        IReadOnlyList<Recipe> recipes,
        IReadOnlyDictionary<Guid, RecipeSection> sectionsById)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return [];
        }

        var query = searchText.Trim();
        var queryTokens = Tokenize(query);
        var results = new List<SearchResult>();

        foreach (var recipe in recipes)
        {
            var sectionMatches = recipe.SectionIds
                .Where(sectionsById.ContainsKey)
                .Select(id => sectionsById[id])
                .ToArray();

            var fields = new List<string>
            {
                recipe.Name,
                recipe.Description,
                recipe.StepsMarkdown
            };

            fields.AddRange(sectionMatches.Select(section => section.Name));
            fields.AddRange(recipe.Ingredients.Select(ingredient => $"{ingredient.Measurement} {ingredient.Name}".Trim()));

            var score = Score(query, queryTokens, fields);
            if (score > 0)
            {
                results.Add(new SearchResult
                {
                    Recipe = recipe,
                    Sections = sectionMatches,
                    Score = score
                });
            }
        }

        return results
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Recipe.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static double Score(string query, string[] queryTokens, IEnumerable<string> fields)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();
        double bestScore = 0;

        foreach (var field in fields.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            var candidate = field.ToLowerInvariant();
            if (candidate.Contains(normalizedQuery, StringComparison.Ordinal))
            {
                bestScore = Math.Max(bestScore, 100 - Math.Min(candidate.Length - normalizedQuery.Length, 24));
            }

            foreach (var token in queryTokens)
            {
                if (candidate.Contains(token, StringComparison.Ordinal))
                {
                    bestScore = Math.Max(bestScore, 65 - Math.Min(candidate.Length - token.Length, 20));
                }
            }

            foreach (var candidateToken in Tokenize(candidate))
            {
                var distance = LevenshteinDistance(normalizedQuery, candidateToken);
                if (distance <= 2)
                {
                    bestScore = Math.Max(bestScore, 50 - (distance * 10));
                }

                foreach (var queryToken in queryTokens)
                {
                    var tokenDistance = LevenshteinDistance(queryToken, candidateToken);
                    if (tokenDistance <= 1)
                    {
                        bestScore = Math.Max(bestScore, 38 - (tokenDistance * 8));
                    }
                }
            }
        }

        return bestScore;
    }

    private static string[] Tokenize(string value) =>
        value
            .Split([' ', ',', '.', ':', ';', '-', '_', '\r', '\n', '\t', '/', '\\', '(', ')'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static int LevenshteinDistance(string source, string target)
    {
        if (source.Length == 0)
        {
            return target.Length;
        }

        if (target.Length == 0)
        {
            return source.Length;
        }

        var matrix = new int[source.Length + 1, target.Length + 1];

        for (var row = 0; row <= source.Length; row++)
        {
            matrix[row, 0] = row;
        }

        for (var column = 0; column <= target.Length; column++)
        {
            matrix[0, column] = column;
        }

        for (var row = 1; row <= source.Length; row++)
        {
            for (var column = 1; column <= target.Length; column++)
            {
                var cost = source[row - 1] == target[column - 1] ? 0 : 1;
                matrix[row, column] = Math.Min(
                    Math.Min(matrix[row - 1, column] + 1, matrix[row, column - 1] + 1),
                    matrix[row - 1, column - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }
}
