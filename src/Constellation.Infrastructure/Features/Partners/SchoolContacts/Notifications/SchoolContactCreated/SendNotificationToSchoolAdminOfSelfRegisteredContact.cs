namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Notifications.SchoolContactCreated;

using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

internal class SendNotificationToSchoolAdminOfSelfRegisteredContact
    : INotificationHandler<SchoolContactCreatedNotification>
{
    private readonly IAppDbContext _context;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly IEmailGateway _emailSender;

    public SendNotificationToSchoolAdminOfSelfRegisteredContact(
        IAppDbContext context,
        IRazorViewToStringRenderer razorService,
        IEmailGateway emailSender)
    {
        _context = context;
        _razorService = razorService;
        _emailSender = emailSender;
    }

    public async Task Handle(SchoolContactCreatedNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.SelfRegistered)
            return;

        // Self registered user. Send Email to school admin.
        var contact = await _context.SchoolContacts
            .Include(contact => contact.Assignments)
            .ThenInclude(role => role.School)
            .FirstOrDefaultAsync(contact => contact.Id == notification.Id, cancellationToken);

        if (contact is null)
            return;

        var role = contact
            .Assignments
            .Where(role => role.DateEntered.HasValue)
            .OrderByDescending(role => role.DateEntered.Value)
            .FirstOrDefault();

        if (role is null)
            return;

        // Send email to school requesting removal
        var viewModel = "<p>A new school contact has been registered via the Schools Portal:</p>";
        viewModel += $"<p><strong>{contact.DisplayName}</strong> is the <strong>{role.Role}</strong> at <strong>{role.School.Name}</strong></p>";
        viewModel += $"<p>This user was registered at <strong>{role.DateEntered.Value.ToLongDateString()}</strong>.";

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>
        {
            { "Aurora College IT Support", "support@aurora.nsw.edu.au" },
            { "Aurora College", "auroracoll-h.school@det.nsw.edu.au" }
        };

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", "New School Contact registered", body);
    }
}
