namespace Constellation.Infrastructure.Templates.Views.Emails.AssessmentProvisions;

using Application.Domains.Compliance.Assessments.Models;
using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class AssessmentProvisionNotificationForFamiliesEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/AssessmentProvisions/AssessmentProvisionNotificationForFamilies.cshtml";
    public override string ViewLocation => _viewLocation;

    public required StudentProvisions Student { get; set; }
}
