namespace Constellation.Infrastructure.Templates.Views.Emails.AssessmentProvisions;

using Application.Domains.Compliance.Assessments.Models;
using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class AssessmentProvisionNotificationForFamiliesEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/AssessmentProvisions/AssessmentProvisionNotificationForFamilies.cshtml";

    public StudentProvisions Student { get; set; }
}
