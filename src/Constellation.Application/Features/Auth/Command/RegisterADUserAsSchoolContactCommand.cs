namespace Constellation.Application.Features.Auth.Command;

using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Core.Models;
using Interfaces.Repositories;
using Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public sealed class RegisterADUserAsSchoolContactCommand : IRequest
{
    public string EmailAddress { get; set; }
}

internal sealed class RegisterADUserAsSchoolContactCommandHandler : IRequestHandler<RegisterADUserAsSchoolContactCommand>
{
    private readonly IActiveDirectoryActionsService _activeDirectoryService;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterADUserAsSchoolContactCommandHandler(
        IActiveDirectoryActionsService activeDirectoryService,
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork)
    {
        _activeDirectoryService = activeDirectoryService;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RegisterADUserAsSchoolContactCommand request, CancellationToken cancellationToken)
    {
        (string FirstName, string LastName) response = _activeDirectoryService.GetUserDetailsFromAD(request.EmailAddress);

        if (response.FirstName is null)
            return Unit.Value;

        SchoolContact contact = new()
        {
            FirstName = response.FirstName, 
            LastName = response.LastName, 
            EmailAddress = request.EmailAddress,
            SelfRegistered = true
        };

        _contactRepository.Insert(contact);
        await _unitOfWork.CompleteAsync(cancellationToken);

        await _mediator.Send(new SchoolContactCreatedNotification { Id = contact.Id }, cancellationToken);

        return Unit.Value;
    }
}
