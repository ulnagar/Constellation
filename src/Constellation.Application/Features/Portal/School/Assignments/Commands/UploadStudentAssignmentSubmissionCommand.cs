using Constellation.Application.Common.ValidationRules;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.Portal.School.Assignments.Commands
{
    public class UploadStudentAssignmentSubmissionCommand : IRequest<ValidateableResponse>
    {
        public Guid AssignmentId { get; set; }
        public string StudentId { get; set; }
        public FileDto File { get; set; }
    }

    public class UploadStudentAssignmentSubmissionCommandValidator : AbstractValidator<UploadStudentAssignmentSubmissionCommand>
    {
        private readonly IAppDbContext _context;

        public UploadStudentAssignmentSubmissionCommandValidator(IAppDbContext context)
        {
            _context = context;

            RuleFor(command => command.AssignmentId).NotEmpty().MustAsync(BeValidAssignmentId);
            RuleFor(command => command.StudentId).NotEmpty().MustAsync(BeValidStudentId);
            RuleFor(command => command.File).NotNull();
        }

        public async Task<bool> BeValidAssignmentId(Guid assignmentId, CancellationToken cancellation = new CancellationToken())
        {
            return await _context.CanvasAssignments.AnyAsync(assignment => assignment.Id == assignmentId, cancellationToken: cancellation);
        }

        public async Task<bool> BeValidStudentId(string studentId, CancellationToken cancellation = new CancellationToken())
        {
            return await _context.Students.AnyAsync(student => student.StudentId == studentId && !student.IsDeleted, cancellationToken: cancellation);
        }
    }
}
