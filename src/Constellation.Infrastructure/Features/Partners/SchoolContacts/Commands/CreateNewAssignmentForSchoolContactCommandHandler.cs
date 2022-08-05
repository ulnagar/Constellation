using Constellation.Application.Features.Partners.SchoolContacts.Commands;
using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Commands
{
    public class CreateNewAssignmentForSchoolContactCommandHandler : IRequestHandler<CreateNewAssignmentForSchoolContactCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public CreateNewAssignmentForSchoolContactCommandHandler(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(CreateNewAssignmentForSchoolContactCommand request, CancellationToken cancellationToken)
        {
            var currentContact = await _context.SchoolContacts
                .FirstOrDefaultAsync(contact => contact.Id == request.ContactId, cancellationToken);

            if (currentContact == null)
                return Unit.Value;

            if (currentContact.IsDeleted)
            {
                currentContact.DateDeleted = null;
                currentContact.IsDeleted = false;
            }

            var currentRoles = await _context.SchoolContactRoles
                .CountAsync(assignment =>
                    !assignment.IsDeleted &&
                    assignment.SchoolContactId == request.ContactId &&
                    assignment.SchoolCode == request.SchoolCode &&
                    assignment.Role == request.Position, cancellationToken);

            if (currentRoles > 0)
            {
                return Unit.Value;
            }

            var assignment = new SchoolContactRole
            {
                SchoolContactId = request.ContactId,
                SchoolCode = request.SchoolCode,
                Role = request.Position,
                DateEntered = DateTime.Now
            };

            _context.Add(assignment);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new SchoolContactRoleAssignmentCreatedNotification { AssignmentId = assignment.Id }, cancellationToken);

            return Unit.Value;
        }
    }
}
