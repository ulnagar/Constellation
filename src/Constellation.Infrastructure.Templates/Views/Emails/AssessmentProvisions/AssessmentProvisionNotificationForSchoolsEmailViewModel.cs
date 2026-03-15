namespace Constellation.Infrastructure.Templates.Views.Emails.AssessmentProvisions;

using Application.Domains.Compliance.Assessments.Models;
using Constellation.Infrastructure.Templates.Views.Shared;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed class AssessmentProvisionNotificationForSchoolsEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/AssessmentProvisions/AssessmentProvisionNotificationForSchools.cshtml";
    public override string ViewLocation => _viewLocation;

    public required Name Contact { get; set; }
    public List<StudentProvisions> Students { get; set; } = [];
}
