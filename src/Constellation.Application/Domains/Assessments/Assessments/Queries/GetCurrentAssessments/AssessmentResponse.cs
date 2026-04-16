namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessments;

using Core.Enums;
using Core.Models.Assessments.Identifiers;
using Core.Models.Subjects.Identifiers;
using System;

public sealed record AssessmentResponse(
    AssessmentId Id,
    string Name,
    CourseId CourseId,
    string Course,
    Grade Grade,
    DateTimeOffset DueDate,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableTo);