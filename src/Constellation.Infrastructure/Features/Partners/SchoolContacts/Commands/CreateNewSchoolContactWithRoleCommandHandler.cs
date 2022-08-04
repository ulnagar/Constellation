using Constellation.Application.Features.Partners.SchoolContacts.Commands;
using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Commands
{
    public class CreateNewSchoolContactWithRoleCommandHandler : IRequestHandler<CreateNewSchoolContactWithRoleCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public CreateNewSchoolContactWithRoleCommandHandler(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(CreateNewSchoolContactWithRoleCommand request, CancellationToken cancellationToken)
        {
            var contact = await _context.SchoolContacts
                .FirstOrDefaultAsync(contact => contact.EmailAddress == request.EmailAddress, cancellationToken);

            if (contact == null)
            {
                contact = new SchoolContact
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    EmailAddress = request.EmailAddress,
                    PhoneNumber = request.PhoneNumber,
                    SelfRegistered = true,
                    DateEntered = DateTime.Now
                };

                _context.SchoolContacts.Add(contact);
            } else
            {
                if (contact.IsDeleted)
                {
                    contact.IsDeleted = false;
                    contact.DateDeleted = null;
                }
            }

            var role = await _context.SchoolContactRoles
                .FirstOrDefaultAsync(role => role.SchoolContact == contact && role.Role == request.Position && role.SchoolCode == request.SchoolCode, cancellationToken);

            if (role == null || role.IsDeleted)
            {
                role = new SchoolContactRole
                {
                    SchoolCode = request.SchoolCode,
                    SchoolContact = contact,
                    Role = request.Position,
                    DateEntered = DateTime.Now
                };

                _context.SchoolContactRoles.Add(role);
            }

            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new SchoolContactCreatedNotification { Id = contact.Id }, cancellationToken);

            return Unit.Value;
        }
    }
}
