namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetAssignmentSubmissionFile;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Core.Models.Assessments.Archive.Identifiers;

public sealed record GetAssignmentSubmissionFileQuery(
    AssignmentId AssignmentId,
    AssignmentSubmissionId SubmissionId)
    : IQuery<FileDto>;