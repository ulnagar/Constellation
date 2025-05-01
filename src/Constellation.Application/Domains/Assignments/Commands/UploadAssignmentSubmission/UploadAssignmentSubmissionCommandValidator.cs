namespace Constellation.Application.Domains.Assignments.Commands.UploadAssignmentSubmission;

using Core.Models.Assignments.Identifiers;
using Core.Models.Assignments.Repositories;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

public class UploadAssignmentSubmissionCommandValidator : AbstractValidator<UploadAssignmentSubmissionCommand>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IStudentRepository _studentRepository;

    public UploadAssignmentSubmissionCommandValidator(
        IAssignmentRepository assignmentRepository,
        IStudentRepository studentRepository)
    {
        _assignmentRepository = assignmentRepository;
        _studentRepository = studentRepository;

        RuleFor(command => command.AssignmentId)
            .NotEmpty()
            .MustAsync(BeValidAssignmentId);

        RuleFor(command => command.StudentId)
            .NotEmpty()
            .MustAsync(BeValidStudentId);

        RuleFor(command => command.File)
            .NotNull();
    }

    public async Task<bool> BeValidAssignmentId(AssignmentId assignmentId, CancellationToken cancellation = new CancellationToken()) => 
        await _assignmentRepository.IsValidAssignmentId(assignmentId, cancellation);

    public async Task<bool> BeValidStudentId(StudentId studentId, CancellationToken cancellation = new CancellationToken()) => 
        await _studentRepository.IsValidStudentId(studentId, cancellation);
}
