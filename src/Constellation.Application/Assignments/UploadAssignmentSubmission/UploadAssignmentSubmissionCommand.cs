namespace Constellation.Application.Assignments.UploadAssignmentSubmission;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Identifiers;

public sealed record UploadAssignmentSubmissionCommand(
    AssignmentId AssignmentId,
    string StudentId,
    FileDto File)
    : ICommand;
