namespace Constellation.Application.Domains.SchoolContacts.Commands.UpdateSchoolContactPhoneNumber;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Serilog;
using System.Threading.Tasks;

internal sealed class UpdateSchoolContactPhoneNumberCommandHandler
: ICommandHandler<UpdateSchoolContactPhoneNumberCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateSchoolContactPhoneNumberCommandHandler(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateSchoolContactPhoneNumberCommand request, CancellationToken cancellationToken)
    {
        SchoolContact? contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
            return Result.Failure(SchoolContactErrors.NotFound(request.ContactId));

        contact.AddPhoneNumber(request.PhoneNumber);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
