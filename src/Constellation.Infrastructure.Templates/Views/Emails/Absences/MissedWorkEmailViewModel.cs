namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public sealed class MissedWorkEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string Subject { get; set; }
    public string ClassName { get; set; }
    public DateOnly AbsenceDate { get; set; }
}
