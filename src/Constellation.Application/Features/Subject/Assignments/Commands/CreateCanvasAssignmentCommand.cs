using Constellation.Application.Common.ValidationRules;
using FluentValidation;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.Features.Subject.Assignments.Commands
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
}
