namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Commands;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.SchoolContacts.CreateContact;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateContactCommandHandler
    : ICommandHandler<CreateContactCommand, int>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public CreateContactCommandHandler(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetWithRolesByEmailAddress(request.EmailAddress, cancellationToken);

        if (contact is not null && contact.IsDeleted)
        {
            contact.IsDeleted = false;
            contact.DateDeleted = null;
        }
        else if (contact is null)
        {
            contact = new SchoolContact
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailAddress = request.EmailAddress,
                PhoneNumber = request.PhoneNumber,
                SelfRegistered = request.SelfRegistered,
                DateEntered = DateTime.Now
            };

            _contactRepository.Insert(contact);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        await _mediator.Publish(new SchoolContactCreatedNotification { Id = contact.Id }, cancellationToken);

        return contact.Id;
    }
}
