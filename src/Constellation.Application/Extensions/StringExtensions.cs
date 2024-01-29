using System.Collections.Generic;
using System.Linq;

namespace Constellation.Application.Extensions
{
    public static class StringExtensions
    {
        public static ICollection<string> Expand(this string oString, char delimiter)
        {
            if (string.IsNullOrWhiteSpace(oString))
                return new List<string>();

            return oString.Split(delimiter).ToList().RemoveNulls().ToList();
        }

        public static string Collapse(this ICollection<string> oList, char delimiter)
        {
            var oString = "";

            var list = oList.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                if (i == list.Count - 1)
                    oString += list[i];
                else
                    oString += list[i] + delimiter;
            }
            
            return oString;
        }

        public static string FormatField(this string content)
        {
            return content.RemoveQuotes().RemoveWhitespace();
        }

        public static string FormatEmail(this string content)
        {
            return content.RemoveQuotes().RemoveWhitespace().ToLower();
        }

        public static string RemoveQuotes(this string content)
        {
            return content.TrimStart('"').TrimEnd('"');
        }

        public static string RemoveWhitespace(this string content)
        {
            return content.TrimStart(' ').TrimEnd(' ');
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
    }
}
