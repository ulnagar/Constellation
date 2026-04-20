namespace Constellation.Application.Domains.Assessments.Assessments.Models;

using Core.Enums;
using Core.Models.Assessments.Identifiers;
using Core.Models.Subjects.Identifiers;

public sealed record AssessmentResponse(
    AssessmentId Id,
    string Name,
    CourseId CourseId,
    string Course,
    Grade Grade,
    DateTimeOffset DueDate,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableTo);