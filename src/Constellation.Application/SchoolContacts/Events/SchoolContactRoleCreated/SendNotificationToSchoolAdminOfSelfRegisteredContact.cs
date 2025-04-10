namespace Constellation.Application.SchoolContacts.Events.SchoolContactRoleCreated;

using Abstractions.Messaging;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Errors;
using Core.Abstractions.Clock;
using Core.Models.SchoolContacts.Events;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Gateways;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationToSchoolAdminOfSelfRegisteredContact
    : IDomainEventHandler<SchoolContactRoleCreatedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly IEmailGateway _emailSender;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendNotificationToSchoolAdminOfSelfRegisteredContact(
        ISchoolContactRepository contactRepository,
        IRazorViewToStringRenderer razorService,
        IEmailGateway emailSender,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _razorService = razorService;
        _emailSender = emailSender;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Handle(SchoolContactRoleCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

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

        SchoolContactRole role = contact.Assignments.FirstOrDefault(role => role.Id == notification.RoleId);

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
        string viewModel = "<p>A new school contact has been registered via the Schools Portal:</p>";
        viewModel += $"<p><strong>{contact.DisplayName}</strong> is the <strong>{role.Role}</strong> at <strong>{role.SchoolName}</strong></p>";
        viewModel += $"<p>This user was registered at <strong>{role.CreatedAt.ToLongDateString()}</strong>.";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.AuroraCollege, EmailRecipient.InfoTechTeam];

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", "New School Contact registered", body, cancellationToken);
    }
}
