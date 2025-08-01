namespace Constellation.Infrastructure.Templates.Views.Emails.AssessmentProvisions;

using Application.Domains.Compliance.Assessments.Models;
using Constellation.Infrastructure.Templates.Views.Shared;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed class AssessmentProvisionNotificationForSchoolsEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/AssessmentProvisions/AssessmentProvisionNotificationForSchools.cshtml";

    public Name Contact { get; set; }
    public List<StudentProvisions> Students { get; set; }
}
