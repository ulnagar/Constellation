namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetAllAssignmentSubmissionFiles;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Core.Models.Assessments.Archive.Identifiers;

public sealed record GetAllAssignmentSubmissionFilesQuery(
        AssignmentId AssignmentId)
    : IQuery<FileDto>;
