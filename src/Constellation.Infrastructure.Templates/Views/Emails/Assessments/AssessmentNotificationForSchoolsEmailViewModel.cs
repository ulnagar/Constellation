namespace Constellation.Infrastructure.Templates.Views.Emails.Assessments;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public sealed class AssessmentNotificationForSchoolsEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Assessments/AssessmentNotificationForSchoolsEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public string PortalLink => BaseUrl;
    public required string CourseName { get; init; }
    public required string AssessmentName { get; init; }
    public required DateOnly DueDate { get; init; }
}
