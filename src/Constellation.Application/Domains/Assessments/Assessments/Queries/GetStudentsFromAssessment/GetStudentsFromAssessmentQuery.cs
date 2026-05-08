namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetStudentsFromAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students;
using System.Collections.Generic;

public sealed record GetStudentsFromAssessmentQuery(
    AssessmentId AssessmentId)
    : IQuery<List<Student>>;