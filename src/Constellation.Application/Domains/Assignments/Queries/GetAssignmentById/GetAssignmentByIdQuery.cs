namespace Constellation.Application.Domains.Assignments.Queries.GetAssignmentById;

using Abstractions.Messaging;
using Core.Models.Assignments.Identifiers;

public sealed record GetAssignmentByIdQuery(
    AssignmentId AssignmentId)
    : IQuery<AssignmentResponse>;