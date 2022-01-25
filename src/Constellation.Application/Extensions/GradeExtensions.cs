using Constellation.Core.Enums;

namespace Constellation.Application.Extensions
{
    public static class GradeExtensions
    {
        public static string AsNumber(this Grade grade)
        {
            return ((int)grade).ToString();
        }
    }
}
