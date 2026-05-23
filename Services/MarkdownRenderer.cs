using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BigBookOfRecipes.Services;

public sealed partial class MarkdownRenderer
{
    private static readonly Regex OrderedListRegex = OrderedList();
    private static readonly Regex UnorderedListRegex = UnorderedList();
    private static readonly Regex BoldRegex = Bold();
    private static readonly Regex ItalicRegex = Italic();
    private static readonly Regex InlineCodeRegex = InlineCode();

    public string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return "<p>No steps added yet.</p>";
        }

        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var builder = new StringBuilder();
        var inOrderedList = false;
        var inUnorderedList = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                CloseLists(builder, ref inOrderedList, ref inUnorderedList);
                continue;
            }

            if (line.StartsWith("#"))
            {
                CloseLists(builder, ref inOrderedList, ref inUnorderedList);
                var level = Math.Min(line.TakeWhile(character => character == '#').Count(), 3);
                var content = line[level..].Trim();
                builder.Append($"<h{level}>{FormatInline(content)}</h{level}>");
                continue;
            }

            var orderedMatch = OrderedListRegex.Match(line);
            if (orderedMatch.Success)
            {
                if (!inOrderedList)
                {
                    CloseLists(builder, ref inOrderedList, ref inUnorderedList);
                    builder.Append("<ol>");
                    inOrderedList = true;
                }

                builder.Append($"<li>{FormatInline(orderedMatch.Groups[1].Value.Trim())}</li>");
                continue;
            }

            var unorderedMatch = UnorderedListRegex.Match(line);
            if (unorderedMatch.Success)
            {
                if (!inUnorderedList)
                {
                    CloseLists(builder, ref inOrderedList, ref inUnorderedList);
                    builder.Append("<ul>");
                    inUnorderedList = true;
                }

                builder.Append($"<li>{FormatInline(unorderedMatch.Groups[1].Value.Trim())}</li>");
                continue;
            }

            CloseLists(builder, ref inOrderedList, ref inUnorderedList);
            builder.Append($"<p>{FormatInline(line.Trim())}</p>");
        }

        CloseLists(builder, ref inOrderedList, ref inUnorderedList);
        return builder.ToString();
    }

    private static void CloseLists(StringBuilder builder, ref bool inOrderedList, ref bool inUnorderedList)
    {
        if (inOrderedList)
        {
            builder.Append("</ol>");
            inOrderedList = false;
        }

        if (inUnorderedList)
        {
            builder.Append("</ul>");
            inUnorderedList = false;
        }
    }

    private static string FormatInline(string text)
    {
        var encoded = WebUtility.HtmlEncode(text);
        encoded = BoldRegex.Replace(encoded, "<strong>$1</strong>");
        encoded = ItalicRegex.Replace(encoded, "<em>$1</em>");
        encoded = InlineCodeRegex.Replace(encoded, "<code>$1</code>");
        return encoded;
    }

    [GeneratedRegex(@"^\d+\.\s+(.+)$", RegexOptions.Compiled)]
    private static partial Regex OrderedList();

    [GeneratedRegex(@"^[-*]\s+(.+)$", RegexOptions.Compiled)]
    private static partial Regex UnorderedList();

    [GeneratedRegex(@"\*\*(.+?)\*\*", RegexOptions.Compiled)]
    private static partial Regex Bold();

    [GeneratedRegex(@"(?<!\*)\*(?!\s)(.+?)(?<!\s)\*(?!\*)", RegexOptions.Compiled)]
    private static partial Regex Italic();

    [GeneratedRegex(@"`(.+?)`", RegexOptions.Compiled)]
    private static partial Regex InlineCode();
}
