namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContactFromActiveDirectory;

using Abstractions.Messaging;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateContactFromActiveDirectoryCommandHandler : ICommandHandler<CreateContactFromActiveDirectoryCommand>
{
    private readonly IActiveDirectoryActionsService _activeDirectoryService;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateContactFromActiveDirectoryCommandHandler(
        IActiveDirectoryActionsService activeDirectoryService,
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _activeDirectoryService = activeDirectoryService;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateContactFromActiveDirectoryCommand>();
    }

    public async Task<Result> Handle(CreateContactFromActiveDirectoryCommand request, CancellationToken cancellationToken)
    {
        (string FirstName, string LastName) response = _activeDirectoryService.GetUserDetailsFromAD(request.EmailAddress);

        if (response.FirstName is null)
        {
            Error error = new("ServiceError", "Failed to retrieve user details from Active Directory");

            _logger
                .ForContext(nameof(CreateContactFromActiveDirectoryCommand), request, true)
                .ForContext(nameof(Error), error, true)
                .Warning("Failed to retrieve user details from Active Directory");

            return Result.Failure(error);
        }

        Result<SchoolContact> contact = SchoolContact.Create(
            response.FirstName,
            response.LastName,
            request.EmailAddress,
            string.Empty,
            true);

        if (contact.IsFailure)
            return Result.Failure(contact.Error);

        _contactRepository.Insert(contact.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}