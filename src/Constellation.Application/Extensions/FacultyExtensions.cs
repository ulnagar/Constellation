using Constellation.Core.Enums;
using System.Collections.Generic;

namespace Constellation.Application.Extensions
{
    public static class FacultyExtensions
    {
        public static ICollection<string> AsList(this Faculty faculty)
        {
            var output = new List<string>();

            if (faculty.HasFlag(Faculty.Administration))
            {
                output.Add("Admin");
            }
            if (faculty.HasFlag(Faculty.Executive))
            {
                output.Add("Exec");
            }
            if (faculty.HasFlag(Faculty.English))
            {
                output.Add("English");
            }
            if (faculty.HasFlag(Faculty.Mathematics))
            {
                output.Add("Maths");
            }
            if (faculty.HasFlag(Faculty.Science))
            {
                output.Add("Science");
            }
            if (faculty.HasFlag(Faculty.Support))
            {
                output.Add("Support");
            }
            if (faculty.HasFlag(Faculty.Stage3))
            {
                output.Add("Stage-3");
            }

            return output;
        }
    }
}
