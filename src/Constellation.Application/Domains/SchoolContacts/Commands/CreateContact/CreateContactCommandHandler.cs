namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContact;

using Abstractions.Messaging;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateContactCommandHandler
    : ICommandHandler<CreateContactCommand, SchoolContactId>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateContactCommandHandler(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SchoolContactId>> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        SchoolContact existingContact = await _contactRepository.GetWithRolesByEmailAddress(request.EmailAddress, cancellationToken);

        if (existingContact is not null)
        {
            if (existingContact.IsDeleted)
                existingContact.Reinstate();

            await _unitOfWork.CompleteAsync(cancellationToken);

            return existingContact.Id;
        }

        Result<SchoolContact> contact = SchoolContact.Create(
            request.FirstName,
            request.LastName,
            request.EmailAddress,
            request.PhoneNumber,
            request.SelfRegistered);

        if (contact.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateContactCommand), request, true)
                .ForContext(nameof(Error), contact.Error, true)
                .Warning("Failed to create new School Contact");

            return Result.Failure<SchoolContactId>(contact.Error);
        }

        _contactRepository.Insert(contact.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return contact.Value.Id;
    }
}
