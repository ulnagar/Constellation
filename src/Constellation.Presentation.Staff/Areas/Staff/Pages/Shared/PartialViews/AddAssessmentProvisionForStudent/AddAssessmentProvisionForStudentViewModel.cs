namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddAssessmentProvisionForStudent;

using Application.Domains.Assessments.Provisions.Models;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed class AddAssessmentProvisionForStudentViewModel
{
    public required StudentId StudentId { get; set; }
    public required Name Student { get; set; }
    public required List<ProvisionId> EnabledProvisionIds { get; set; } = [];
    public required List<AssessmentProvisionResponse> Provisions { get; set; } = [];
}
