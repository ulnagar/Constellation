namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetAssignmentById;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Assessments.Archive.Identifiers;

public sealed record GetAssignmentByIdQuery(
    AssignmentId AssignmentId)
    : IQuery<AssignmentResponse>;