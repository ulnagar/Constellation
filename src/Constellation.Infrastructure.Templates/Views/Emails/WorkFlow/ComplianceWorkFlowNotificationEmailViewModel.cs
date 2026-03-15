namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Shared;

public sealed class ComplianceWorkFlowNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/WorkFlow/ComplianceWorkFlowNotificationEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public string Link => $"{BaseUrl}{LinkPart}";

    public required string Assignee { get; set; }

    public required string StudentName { get; set; }
    public required string StudentGrade { get; set; }
    public required string StudentSchool { get; set; }

    public required string IncidentType { get; set; }
    public required string IncidentId { get; set; }
    public required string IncidentLink { get; set; }
    public required string Subject { get; set; }
    public required int Age { get; set; }

    public required string LinkPart { get; set; }
}
