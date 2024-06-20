namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Application.DTOs.EmailRequests;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Core.Models.WorkFlow;
using Shared;

public sealed class ComplianceWorkFlowNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/WorkFlow/ComplianceWorkFlowNotificationEmail.cshtml";

    public ComplianceCaseDetail Detail { get; set; }

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
