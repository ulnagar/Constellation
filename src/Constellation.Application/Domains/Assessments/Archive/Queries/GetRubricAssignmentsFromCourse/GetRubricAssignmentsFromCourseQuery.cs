namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetRubricAssignmentsFromCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetRubricAssignmentsFromCourseQuery(
    OfferingId OfferingId)
    : IQuery<List<AssignmentFromCourseResponse>>;