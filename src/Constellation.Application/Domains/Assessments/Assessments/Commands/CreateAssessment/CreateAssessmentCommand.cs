namespace Constellation.Application.Domains.Assessments.Assessments.Commands.CreateAssessment;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Assessments.Identifiers;
using Core.Models.Subjects.Identifiers;
using System;

public sealed record CreateAssessmentCommand(
    string Name,
    CourseId CourseId,
    DateTimeOffset DueDate,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableTo)
    : ICommand<AssessmentId>;
