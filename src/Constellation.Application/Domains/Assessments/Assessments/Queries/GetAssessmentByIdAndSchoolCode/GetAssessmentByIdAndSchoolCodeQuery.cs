namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentByIdAndSchoolCode;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Identifiers;
using Models;

public sealed record GetAssessmentByIdAndSchoolCodeQuery(
    AssessmentId AssessmentId,
    SchoolCode SchoolCode)
    : IQuery<AssessmentDetailsResponse>;