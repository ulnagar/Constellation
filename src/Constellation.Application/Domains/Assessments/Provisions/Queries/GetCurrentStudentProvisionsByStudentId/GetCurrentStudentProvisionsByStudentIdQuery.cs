namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetCurrentStudentProvisionsByStudentId;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentStudentProvisionsByStudentIdQuery(
    StudentId StudentId,
    AssessmentId? AssessmentId = null)
    : IQuery<List<AssessmentProvisionResponse>>;
