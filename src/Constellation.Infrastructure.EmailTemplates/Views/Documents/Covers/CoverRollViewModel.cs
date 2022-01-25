using Constellation.Core.Models;
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

            public static EnrolledStudent ConvertFromEnrolment(Enrolment enrolment)
            {
                var viewModel = new EnrolledStudent
                {
                    Name = enrolment.Student.DisplayName,
                    Gender = enrolment.Student.Gender,
                    School = enrolment.Student.School.Name,
                    OrderName = $"{enrolment.Student.LastName} {enrolment.Student.FirstName}"
                };

                return viewModel;
            }
        }
    }
}
