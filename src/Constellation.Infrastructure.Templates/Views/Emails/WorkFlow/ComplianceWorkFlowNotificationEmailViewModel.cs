namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Shared;

public sealed class ComplianceWorkFlowNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/WorkFlow/ComplianceWorkFlowNotificationEmail.cshtml";

    public string StudentName { get; set; }
    public string StudentGrade { get; set; }
    public string StudentSchool { get; set; }

    public string IncidentType { get; set; }
    public string IncidentId { get; set; }
    public string IncidentLink { get; set; }
    public string Subject { get; set; }
    public int Age { get; set; }

    public string Link { get; set; }
}
