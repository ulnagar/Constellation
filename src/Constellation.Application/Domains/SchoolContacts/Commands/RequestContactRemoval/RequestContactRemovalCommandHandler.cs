namespace Constellation.Application.Domains.SchoolContacts.Commands.RequestContactRemoval;

using Abstractions.Messaging;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Interfaces.Services;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RequestContactRemovalCommandHandler 
    : ICommandHandler<RequestContactRemovalCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public RequestContactRemovalCommandHandler(
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        ILogger logger) 
    {
        _contactRepository = contactRepository;
        _emailService = emailService;
        _logger = logger.ForContext<RequestContactRemovalCommand>();
    }
    
    public async Task<Result> Handle(RequestContactRemovalCommand request, CancellationToken cancellationToken)
    {
        SchoolContact? contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(RequestContactRemovalCommand), request, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(request.ContactId), true)
                .Warning("Failed to send request to remove school contact");

            return Result.Failure(SchoolContactErrors.NotFound(request.ContactId));
        }

        SchoolContactRole? role = contact.Assignments.FirstOrDefault(role => role.Id == request.RoleId);

        if (role is null)
        {
            _logger
                .ForContext(nameof(RequestContactRemovalCommand), request, true)
                .ForContext(nameof(Error), SchoolContactRoleErrors.NotFound(request.RoleId), true)
                .Warning("Failed to send request to remove school contact");

            return Result.Failure(SchoolContactRoleErrors.NotFound(request.RoleId));
        }

        return await _emailService.SendSchoolContactRemovalRequest(contact, role, request.CancelledBy, request.CancelledAt, request.Comment);
    }
}