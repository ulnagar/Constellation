namespace Constellation.Application.Domains.Assignments.Queries.GetAssignmentSubmissionFile;

using Abstractions.Messaging;
using Core.Models.Assignments.Identifiers;
using DTOs;

public sealed record GetAssignmentSubmissionFileQuery(
    AssignmentId AssignmentId,
    AssignmentSubmissionId SubmissionId)
    : IQuery<FileDto>;