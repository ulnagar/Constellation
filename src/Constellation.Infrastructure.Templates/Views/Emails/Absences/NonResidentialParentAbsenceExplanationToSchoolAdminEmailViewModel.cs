namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

public sealed class NonResidentialParentAbsenceExplanationToSchoolAdminEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/Absences/NonResidentialParentAbsenceExplanationToSchoolAdminEmail.cshtml";

    public string StudentName { get; set; }
    public List<AbsenceDto> Absences { get; set; } = new();

    public class AbsenceDto
    {
        public DateTime AbsenceDate { get; set; }
        public string ClassName { get; set; }
        public string PeriodName { get; set; }
        public string Explanation { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string AbsenceTime { get; set; }
    }
}