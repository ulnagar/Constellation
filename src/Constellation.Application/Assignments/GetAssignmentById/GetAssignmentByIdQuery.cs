namespace Constellation.Application.Assignments.GetAssignmentById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assignments.Identifiers;

public sealed record GetAssignmentByIdQuery(
    AssignmentId AssignmentId)
    : IQuery<AssignmentResponse>;