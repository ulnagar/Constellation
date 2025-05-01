namespace Constellation.Application.Domains.Assignments.Commands.UploadAssignmentSubmission;

using Abstractions.Messaging;
using Core.Models.Assignments.Identifiers;
using Core.Models.Students.Identifiers;
using DTOs;

public sealed record UploadAssignmentSubmissionCommand(
        AssignmentId AssignmentId,
        StudentId StudentId,
        FileDto File)
    : ICommand
{
    public string SubmittedBy { get; set; }
}
