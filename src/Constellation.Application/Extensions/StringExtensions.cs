namespace Constellation.Application.Extensions;

using Helpers;

public static class StringExtensions
{
    public static string FormatField(this string content)
    {
        return content.RemoveQuotes().RemoveWhitespace();
    }

    public static string FormatEmail(this string content)
    {
        return content.RemoveQuotes().RemoveWhitespace().ToLower();
    }

    public static (string, string) ExtractLine(this string offeringName)
    {
        if (string.IsNullOrWhiteSpace(offeringName))
            return ("Unknown", "Unknown");

        if (offeringName.Length != 7)
            return ("Unknown", "Unknown");

        string line = offeringName.Substring(offeringName.Length - 2, 1);

        return line switch
        {
            "G" => ("Secondary", "G"),
            "P" => ("Secondary", "P"),
            "V" => ("Alternate", "V"),
            "Y" => ("Alternate", "Y"),
            "A" => ("Primary", "A"),
            "B" => ("Primary", "B"),
            "C" => ("Primary", "C"),
            "1" => ("Senior", "1"),
            "2" => ("Senior", "2"),
            "3" => ("Senior", "3"),
            "4" => ("Senior", "4"),
            "5" => ("Senior", "5"),
            "6" => ("Senior", "6"),
            _ => ("Unknown", "Unknown")
        };
    }

    private static string RemoveQuotes(this string content)
    {
        return content.TrimStart('"').TrimEnd('"');
    }

    private static string RemoveWhitespace(this string content)
    {
        return content.TrimStart(' ').TrimEnd(' ');
    }

    /// <summary>
    ///  Extension Method to Remove HTML Tags from a given String.
    /// </summary>
    /// <param name="source">Sourch HTML String</param>
    /// <returns>Plain Text</returns>
    public static string RemoveHtml(this string source)
    {
        return HtmlHelper.RemoveHtmlTags(source);
    }

    /// <summary>
    /// Extension Method to Return a copy of this string converted to HTML markup.
    /// </summary>
    public static string ToHtml(this string source)
    {
        return ToHtml(source, false);
    }

    /// <summary>
    /// Extension Method to Return a copy of this string converted to HTML markup.
    /// </summary>
    /// <param name="source">Non-HTML Text Source String</param>
    /// <param name="nofollow">If true, links are given "nofollow"
    /// attribute</param>
    public static string ToHtml(this string source, bool nofollow)
    {
        // Return HTML Version
        return HtmlHelper.ConvertToHtml(source, nofollow);
    }
}