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

            foreach (var oItem in oList)
                oString += $"{oItem}{delimiter}";

            return oString;
        }
    }
}
