namespace Constellation.Application.Helpers;

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

public static class HtmlHelper
{
    private static readonly string _paraBreak = "\r\n\r\n";
    private static readonly string _link = "<a href=\"{0}\">{1}</a>";
    private static readonly string _linkNoFollow = "<a href=\"{0}\" rel=\"nofollow\">{1}</a>";

    /// <summary>
    /// Remove HTML tags from the given string.
    /// </summary>
    public static string RemoveHtmlTags(string source)
    {
        char[] array = new char[source.Length];
        int arrayIndex = 0;
        bool inside = false;

        for (int i = 0; i < source.Length; i++)
        {
            char let = source[i];
            if (let == '<')
            {
                inside = true;
                continue;
            }
            if (let == '>')
            {
                inside = false;
                continue;
            }
            if (!inside)
            {
                array[arrayIndex] = let;
                arrayIndex++;
            }
        }

        string plainText = new string(array, 0, arrayIndex).Replace("&nbsp;", string.Empty);

        return plainText;
    }

    /// <summary>
    /// Convert a Plain Text String to its HTML equivalent.
    /// </summary>
    /// <param name="source">Source Plain Text String</param>
    /// <param name="nofollow">Boolean Flag to follow through Paragraphs.</param>
    /// <returns>HTML String</returns>
    public static string ConvertToHtml(string source, bool nofollow)
    {
        StringBuilder sb = new();

        int pos = 0;
        while (pos < source.Length)
        {
            // Extract next paragraph
            int start = pos;
            pos = source.IndexOf(_paraBreak, start);
            if (pos < 0)
                pos = source.Length;
            string para = source.Substring(start, pos - start).Trim();

            // Encode non-empty paragraph
            if (para.Length > 0)
                EncodeParagraph(para, sb, nofollow);

            // Skip over paragraph break
            pos += _paraBreak.Length;
        }

        // Get HTML Version
        string html = sb.ToString();

        // Convert URL Addresses to HTML Anchor Tags
        html = Regex.Replace(html,
            @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
            "<a target='_blank' href='$1'>$1</a>");

        // Return HTML Version
        return html;
    }

    /// <summary>
    /// Encodes a single paragraph to HTML.
    /// </summary>
    /// <param name="s">Text to encode</param>
    /// <param name="sb">StringBuilder to write results</param>
    /// <param name="nofollow">If true, links are given "nofollow"
    /// attribute</param>
    private static void EncodeParagraph(string s, StringBuilder sb, bool nofollow)
    {
        // Start new paragraph
        sb.AppendLine("<p>");

        // HTML encode text
        s = HttpUtility.HtmlEncode(s);

        // Convert single newlines to <br>
        s = s.Replace(Environment.NewLine, "<br />\r\n");

        // Encode any hyperlinks
        EncodeLinks(s, sb, nofollow);

        // Close paragraph
        sb.AppendLine("\r\n</p>");
    }

    /// <summary>
    /// Encodes [[URL]] and [[Text][URL]] links to HTML.
    /// </summary>
    /// <param name="text">Text to encode</param>
    /// <param name="sb">StringBuilder to write results</param>
    /// <param name="nofollow">If true, links are given "nofollow"
    /// attribute</param>
    private static void EncodeLinks(string s, StringBuilder sb, bool nofollow)
    {
        // Parse and encode any hyperlinks
        int pos = 0;
        while (pos < s.Length)
        {
            // Look for next link
            int start = pos;
            pos = s.IndexOf("[[", pos);
            if (pos < 0)
                pos = s.Length;
            // Copy text before link
            sb.Append(s.Substring(start, pos - start));
            if (pos < s.Length)
            {
                string label, link;

                start = pos + 2;
                pos = s.IndexOf("]]", start);
                if (pos < 0)
                    pos = s.Length;
                label = s.Substring(start, pos - start);
                int i = label.IndexOf("][");
                if (i >= 0)
                {
                    link = label.Substring(i + 2);
                    label = label.Substring(0, i);
                }
                else
                {
                    link = label;
                }
                // Append link
                sb.Append(String.Format(nofollow ? _linkNoFollow : _link, link, label));

                // Skip over closing "]]"
                pos += 2;
            }
        }
    }
}