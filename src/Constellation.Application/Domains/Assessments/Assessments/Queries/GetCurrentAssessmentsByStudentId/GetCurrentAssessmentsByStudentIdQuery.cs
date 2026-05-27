namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsByStudentId;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentAssessmentsByStudentIdQuery(
    StudentId StudentId)
    : IQuery<List<AssessmentDetailsResponse>>;
