namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddStudentToAssessment;

using Constellation.Core.Models.Students.Identifiers;
using Core.Models.Assessments.Identifiers;

public sealed record AddStudentToAssessmentSelection(
    AssessmentId Id,
    string AssessmentName,
    Dictionary<StudentId, string> Students);
