using Constellation.Core.Models.Enrolments;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Documents.Covers
{
    public class CoverRollViewModel
    {
        public CoverRollViewModel()
        {
            Students = new List<EnrolledStudent>();
        }

        public string ClassName { get; set; }
        public ICollection<EnrolledStudent> Students { get; set; }

        public class EnrolledStudent
        {
            public string Name { get; set; }
            public string Gender { get; set; }
            public string School { get; set; }
            public string OrderName { get; set; }
        }
    }
}
