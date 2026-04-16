namespace Constellation.Application.Domains.Assessments.Assessments.Commands.UpdateAssessment;

using Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Assessments.Identifiers;
using System;

public sealed record UpdateAssessmentCommand(
    AssessmentId Id,
    string Name,
    CourseId CourseId,
    DateTimeOffset DueDate,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableTo)
    : ICommand;
