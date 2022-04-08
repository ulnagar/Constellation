using Constellation.Application.Common.ValidationRules;
using Constellation.Application.Features.Subject.Assignments.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Subjects.Assignments.Commands
{
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
