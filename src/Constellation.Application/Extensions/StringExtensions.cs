namespace Constellation.Application.Extensions;

using Helpers;

public static class StringExtensions
{
    extension(string content)
    {
        public string FormatField => content.RemoveQuotes.RemoveWhitespace;

        public string FormatEmail => content.RemoveQuotes.RemoveWhitespace;

        public (string, string) ExtractLine()
        {
            if (string.IsNullOrWhiteSpace(content))
                return ("Unknown", "Unknown");

            if (content.Length != 7)
                return ("Unknown", "Unknown");

            string line = content.Substring(content.Length - 2, 1);

            return line switch
            {
                "G" => ("Secondary", "G"),
                "N" => ("Secondary", "N"),

                "V" => ("Alternate", "V"),
                "Y" => ("Alternate", "Y"),

                "B" => ("Primary", "B"),
                "P" => ("Primary", "P"),
                "R" => ("Primary", "R"),

                "1" => ("Senior", "1"),
                "2" => ("Senior", "2"),
                "3" => ("Senior", "3"),
                "4" => ("Senior", "4"),
                "5" => ("Senior", "5"),
                "6" => ("Senior", "6"),
                _ => ("Unknown", "Unknown")
            };
        }

        public string ToHtml()
        {
            return ToHtml(content, false);
        }

        public string ToHtml(bool noFollow)
        {
            return HtmlHelper.ConvertToHtml(content, noFollow);
        }

        private string RemoveQuotes => content.TrimStart('"').TrimEnd('"');
        
        private string RemoveWhitespace => content.TrimStart(' ').TrimEnd(' ');
    }
}