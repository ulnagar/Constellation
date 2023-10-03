namespace Constellation.Application.Assignments.UploadAssignmentSubmission;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Assignments.Identifiers;

public sealed record UploadAssignmentSubmissionCommand(
        AssignmentId AssignmentId,
        string StudentId,
        FileDto File)
    : ICommand
{
    public string SubmittedBy { get; set; }
}
