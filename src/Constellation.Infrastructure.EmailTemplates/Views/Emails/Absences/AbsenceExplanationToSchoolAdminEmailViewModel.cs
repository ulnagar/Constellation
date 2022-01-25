using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Emails.Absences
{
    public class AbsenceExplanationToSchoolAdminEmailViewModel : EmailLayoutBaseViewModel
    {
        public AbsenceExplanationToSchoolAdminEmailViewModel()
        {
            Absences = new List<AbsenceDto>();
        }

        public string StudentName { get; set; }
        public ICollection<AbsenceDto> Absences { get; set; }

        public class AbsenceDto
        {
            public DateTime AbsenceDate { get; set; }
            public string ClassName { get; set; }
            public string PeriodName { get; set; }
            public string Explanation { get; set; }
            public string Source { get; set; }
        }
    }
}
