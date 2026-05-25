namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsBySchoolCode;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentAssessmentsBySchoolCodeQuery(
    SchoolCode SchoolCode)
    : IQuery<List<AssessmentDetailsResponse>>;
