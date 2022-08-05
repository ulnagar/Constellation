using Constellation.Application.Features.Partners.SchoolContacts.Commands;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
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
            var contactId = await _mediator.Send(new CreateNewSchoolContactCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                EmailAddress = request.EmailAddress
            }, cancellationToken);

            await _mediator.Send(new CreateNewAssignmentForSchoolContactCommand
            {
                ContactId = contactId,
                SchoolCode = request.SchoolCode,
                Position = request.Position
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
