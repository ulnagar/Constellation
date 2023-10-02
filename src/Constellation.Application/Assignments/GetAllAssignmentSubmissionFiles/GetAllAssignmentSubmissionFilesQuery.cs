namespace Constellation.Application.Assignments.GetAllAssignmentSubmissionFiles;

using Abstractions.Messaging;
using Core.Models.Assignments.Identifiers;
using DTOs;

public sealed record GetAllAssignmentSubmissionFilesQuery(
        AssignmentId AssignmentId)
    : IQuery<FileDto>;
