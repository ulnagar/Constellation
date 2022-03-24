using Constellation.Application.Common.CQRS.Gateways.CanvasGateway.Notifications;
using Constellation.Application.Common.ValidationRules;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Portal.School.Assignment.Commands
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

    public class UploadStudentAssignmentSubmissionCommandHandler : IRequestHandler<UploadStudentAssignmentSubmissionCommand, ValidateableResponse>
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public UploadStudentAssignmentSubmissionCommandHandler(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<ValidateableResponse> Handle(UploadStudentAssignmentSubmissionCommand request, CancellationToken cancellationToken)
        {
            using var transaction = _context.Database.BeginTransaction();
            var attempts = await _context.CanvasAssignmentsSubmissions
                .CountAsync(submission => submission.AssignmentId == request.AssignmentId && submission.StudentId == request.StudentId);

            try
            {
                var record = new CanvasAssignmentSubmission
                {
                    AssignmentId = request.AssignmentId,
                    StudentId = request.StudentId,
                    Attempt = attempts + 1,
                    SubmittedDate = DateTime.Now
                };

                _context.CanvasAssignmentsSubmissions.Add(record);
                await _context.SaveChangesAsync(cancellationToken);

                var file = new StoredFile
                {
                    LinkId = record.Id.ToString(),
                    LinkType = StoredFile.CanvasAssignmentSubmission,
                    Name = request.File.FileName,
                    FileType = request.File.FileType,
                    FileData = request.File.FileData,
                    CreatedAt = DateTime.Now
                };

                _context.StoredFiles.Add(file);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                await _mediator.Publish(new CanvasAssignmentSubmissionUploadedNotification { Id = record.Id });
            }
            catch (Exception ex)
            {
                return new ValidateableResponse(new List<string> { ex.Message });
            }

            return new ValidateableResponse();
        }
    }
}
