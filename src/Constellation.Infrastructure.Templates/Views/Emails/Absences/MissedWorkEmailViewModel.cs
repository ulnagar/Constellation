namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public sealed class MissedWorkEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/MissedWorkEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string StudentName { get; set; }
    public required string Subject { get; set; }
    public required string ClassName { get; set; }
    public required DateOnly AbsenceDate { get; set; }
}
