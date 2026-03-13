namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertResponseToAbsenceExplanation;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public sealed class CoordinatorAbsenceVerificationRequestEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/CoordinatorAbsenceVerificationRequestEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public const string PortalLink = $"{BaseUrl}";

    public required string StudentName { get; set; }
    public required string SchoolName { get; set; }
    public List<AbsenceExplanation> ClassList { get; set; } = [];
}
