namespace Constellation.Application.SchoolContacts.RemoveContactRole;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveContactRoleCommandHandler
    : ICommandHandler<RemoveContactRoleCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveContactRoleCommandHandler(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RemoveContactRoleCommand>();
    }

    public async Task<Result> Handle(RemoveContactRoleCommand request, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(RemoveContactRoleCommand), request, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(request.ContactId), true)
                .Warning("Failed to remove School Contact Role");

            return Result.Failure(SchoolContactErrors.NotFound(request.ContactId));
        }

        SchoolContactRole role = contact.Assignments.FirstOrDefault(role => role.Id == request.RoleId);

        if (role is null)
        {
            _logger
                .ForContext(nameof(RemoveContactRoleCommand), request, true)
                .ForContext(nameof(SchoolContact), contact, true)
                .ForContext(nameof(Error), SchoolContactRoleErrors.NotFound(request.RoleId), true)
                .Warning("Failed to remove School Contact Role");

            return Result.Failure(SchoolContactRoleErrors.NotFound(request.RoleId));
        }

        Result attempt = contact.RemoveRole(request.RoleId);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(RemoveContactRoleCommand), request, true)
                .ForContext(nameof(SchoolContact), contact, true)
                .ForContext(nameof(SchoolContactRole), role, true)
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to remove School Contact Role");

            return attempt;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
