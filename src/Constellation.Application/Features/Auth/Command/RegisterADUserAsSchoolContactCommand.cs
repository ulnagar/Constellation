namespace Constellation.Application.Features.Auth.Command;

using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class RegisterADUserAsSchoolContactCommand : IRequest
{
    public string EmailAddress { get; set; }
}

public class RegisterADUserAsSchoolContactCommandHandler : IRequestHandler<RegisterADUserAsSchoolContactCommand>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;
    private readonly IActiveDirectoryGateway _gateway;

    public RegisterADUserAsSchoolContactCommandHandler(IAppDbContext context, IMediator mediator, IActiveDirectoryGateway gateway)
    {
        _context = context;
        _mediator = mediator;
        _gateway = gateway;
    }

    public async Task<Unit> Handle(RegisterADUserAsSchoolContactCommand request, CancellationToken cancellationToken)
    {
        var userLookup = await _gateway.GetUserDetailsFromActiveDirectory(request.EmailAddress);

        if (!string.IsNullOrWhiteSpace(userLookup.Email))
        {
            var contact = new SchoolContact
            {
                FirstName = userLookup.FirstName,
                LastName = userLookup.LastName,
                EmailAddress = userLookup.Email
            };

            _context.Add(contact);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Send(new SchoolContactCreatedNotification { Id = contact.Id }, cancellationToken);
        }

        return Unit.Value;
    }
}
