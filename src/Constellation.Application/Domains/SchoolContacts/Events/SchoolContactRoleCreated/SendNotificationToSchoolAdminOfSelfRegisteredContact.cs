namespace Constellation.Application.Domains.SchoolContacts.Events.SchoolContactRoleCreated;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Events;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Interfaces.Services;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationToSchoolAdminOfSelfRegisteredContact
    : IDomainEventHandler<SchoolContactRoleCreatedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendNotificationToSchoolAdminOfSelfRegisteredContact(
        ISchoolContactRepository contactRepository,
        IDateTimeProvider dateTime,
        IEmailService emailService,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _dateTime = dateTime;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(SchoolContactRoleCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        SchoolContact? contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to send notice to Admin of new self registered School Contact");

            return;
        }

        if (!contact.SelfRegistered || _dateTime.Now.Subtract(contact.CreatedAt).TotalDays > 5)
            return;

        SchoolContactRole? role = contact.Assignments.FirstOrDefault(role => role.Id == notification.RoleId);

        if (role is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                .ForContext(nameof(SchoolContact), contact, true)
                .ForContext(nameof(Error), SchoolContactRoleErrors.NotFound(notification.RoleId), true)
                .Warning("Failed to send notice to Admin of new self registered School Contact");

            return;
        }

        // Send email to school requesting removal
        await _emailService.SendSchoolContactAddedNotification(contact, role);
    }
}
