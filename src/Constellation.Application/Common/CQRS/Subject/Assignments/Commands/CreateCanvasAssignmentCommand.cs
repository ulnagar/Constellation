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
    public class CreateCanvasAssignmentCommand : IRequest
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public int CanvasId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LockDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? UnlockDate { get; set; }
        public int AllowedAttempts { get; set; }
    }

    public class CreateCanvasAssignmentCommandValidator : AbstractValidator<CreateCanvasAssignmentCommand>
    {
        public CreateCanvasAssignmentCommandValidator()
        {
        }
    }

    public class CreateCanvasAssignmentCommandHandler : IRequestHandler<CreateCanvasAssignmentCommand>
    {
        private readonly IAppDbContext _context;

        public CreateCanvasAssignmentCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CreateCanvasAssignmentCommand request, CancellationToken cancellationToken)
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

            return Unit.Value;
        }
    }
}
