namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

public sealed class AbsenceExplanationToSchoolAdminEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/AbsenceExplanationToSchoolAdminEmail.cshtml";
    public override string ViewLocation => _viewLocation;


    public required string StudentName { get; set; } = string.Empty;
    public List<AbsenceDto> Absences { get; set; } = [];

    public class AbsenceDto
    {
        public required DateTime AbsenceDate { get; set; }
        public required string ClassName { get; set; }
        public required string PeriodName { get; set; }
        public required string Explanation { get; set; }
        public required string Source { get; set; }
        public required string Type { get; set; }
        public required string AbsenceTime { get; set; }
    }
}