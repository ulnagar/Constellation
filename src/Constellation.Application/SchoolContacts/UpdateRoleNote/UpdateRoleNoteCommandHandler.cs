namespace Constellation.Application.SchoolContacts.UpdateRoleNote;

using Abstractions.Messaging;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateRoleNoteCommandHandler
: ICommandHandler<UpdateRoleNoteCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateRoleNoteCommandHandler(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateRoleNoteCommand>();
    }

    public async Task<Result> Handle(UpdateRoleNoteCommand request, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(UpdateRoleNoteCommand), request, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(request.ContactId), true)
                .Warning("Failed to update School Contact Role note");

            return Result.Failure(SchoolContactErrors.NotFound(request.ContactId));
        }

        Result attempt = contact.UpdateRoleNote(request.RoleId, request.Note);

        if (attempt.IsSuccess)
            await _unitOfWork.CompleteAsync(cancellationToken);

        return attempt;
    }
}
