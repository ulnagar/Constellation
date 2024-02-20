namespace Constellation.Application.SchoolContacts.UpdateContact;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateContactCommandHandler
    : ICommandHandler<UpdateContactCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateContactCommandHandler(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateContactCommand>();
    }

    public async Task<Result> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger.Warning("Could not find School Contact with Id {id}", request.ContactId);

            return Result.Failure(SchoolContactErrors.NotFound(request.ContactId));
        }

        Result<Name> contactName = Name.Create(request.FirstName, string.Empty, request.LastName);

        if (contactName.IsFailure)
        {
            _logger
                .ForContext(nameof(Result.Error), contactName.Error, true)
                .Warning("Could not create name for School Contact with Id {id}", request.ContactId);

            return Result.Failure(contactName.Error);
        }

        Result<EmailAddress> contactEmail = EmailAddress.Create(request.EmailAddress);

        if (contactEmail.IsFailure)
        {
            _logger
                .ForContext(nameof(Result.Error), contactEmail.Error, true)
                .Warning("Could not create email address for School Contact with Id {id}", request.ContactId);

            return Result.Failure(contactName.Error);
        }

        Result<PhoneNumber> contactPhone = PhoneNumber.Create(request.PhoneNumber);

        if (contactPhone.IsFailure && !string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            _logger
                .ForContext(nameof(Result.Error), contactPhone.Error, true)
                .Warning("Could not create phone number for School Contact with Id {id}", request.ContactId);

            return Result.Failure(contactPhone.Error);
        }

        contact.Update(
            contactName.Value.FirstName,
            contactName.Value.LastName,
            contactEmail.Value.Email,
            contactPhone.IsSuccess ? contactPhone.Value.ToString(PhoneNumber.Format.None) : string.Empty);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
