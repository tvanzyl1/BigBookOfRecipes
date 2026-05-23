using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BigBookOfRecipes.Services;

public sealed partial class SlugService
{
    private static readonly Regex InvalidCharsRegex = InvalidChars();
    private static readonly Regex MultiDashRegex = MultiDash();

    public string GenerateSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "item";
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var cleaned = InvalidCharsRegex.Replace(builder.ToString(), "-");
        cleaned = MultiDashRegex.Replace(cleaned, "-").Trim('-');

        return string.IsNullOrWhiteSpace(cleaned) ? "item" : cleaned;
    }

    public string EnsureUniqueSlug(string baseValue, IEnumerable<string> existingSlugs, string? currentSlug = null)
    {
        var slugBase = GenerateSlug(baseValue);
        var occupied = existingSlugs
            .Where(slug => !string.Equals(slug, currentSlug, StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!occupied.Contains(slugBase))
        {
            return slugBase;
        }

        var suffix = 2;
        while (occupied.Contains($"{slugBase}-{suffix}"))
        {
            suffix++;
        }

        return $"{slugBase}-{suffix}";
    }

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex InvalidChars();

    [GeneratedRegex(@"-+", RegexOptions.Compiled)]
    private static partial Regex MultiDash();
}
