using Constellation.Application.Common.ValidationRules;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using FluentValidation;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Subject.Assignments.Commands
{
    public class CreateCanvasAssignmentCommand : IRequest<ValidateableResponse>, IValidatable
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public int CanvasId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; } = DateTime.Now;
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LockDate { get; set; } = DateTime.Now;
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? UnlockDate { get; set; } = DateTime.Now;
        public int AllowedAttempts { get; set; }
    }

    public class CreateCanvasAssignmentCommandValidator : AbstractValidator<CreateCanvasAssignmentCommand>
    {
        public CreateCanvasAssignmentCommandValidator()
        {
            RuleFor(command => command.DueDate).GreaterThanOrEqualTo(DateTime.Today).WithMessage($"Due Date must be in the future!");
            RuleFor(command => command.UnlockDate).LessThanOrEqualTo(command => command.DueDate).LessThanOrEqualTo(command => command.LockDate).WithMessage($"Unlock Date must be before the Due Date and the Lock Date!");
            RuleFor(command => command.LockDate).GreaterThanOrEqualTo(command => command.UnlockDate).WithMessage($"Lock Date must be after the Unlock Date!");
        }
    }

    public class CreateCanvasAssignmentCommandHandler : IRequestHandler<CreateCanvasAssignmentCommand, ValidateableResponse>
    {
        private readonly IAppDbContext _context;

        public CreateCanvasAssignmentCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ValidateableResponse> Handle(CreateCanvasAssignmentCommand request, CancellationToken cancellationToken)
        {
            var record = new CanvasAssignment
            {
                CanvasId = request.CanvasId,
                Name = request.Name,
                CourseId = request.CourseId,
                DueDate = request.DueDate,
                LockDate = request.LockDate,
                UnlockDate = request.UnlockDate,
                AllowedAttempts = request.AllowedAttempts
            };

            _context.CanvasAssignments.Add(record);
            await _context.SaveChangesAsync(cancellationToken);

            return new ValidateableResponse();
        }
    }
}
