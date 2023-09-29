namespace Constellation.Application.Assignments.GetAssignmentSubmissionFile;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Assignments.Identifiers;

public sealed record GetAssignmentSubmissionFileQuery(
    AssignmentId AssignmentId,
    AssignmentSubmissionId SubmissionId)
    : IQuery<FileDto>;