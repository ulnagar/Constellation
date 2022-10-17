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
    }
}
