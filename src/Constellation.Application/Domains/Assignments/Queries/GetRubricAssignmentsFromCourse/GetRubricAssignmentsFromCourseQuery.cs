namespace Constellation.Application.Domains.Assignments.Queries.GetRubricAssignmentsFromCourse;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetRubricAssignmentsFromCourseQuery(
    OfferingId OfferingId)
    : IQuery<List<AssignmentFromCourseResponse>>;