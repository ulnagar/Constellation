namespace Constellation.Application.SchoolContacts.RequestContactRemoval;

using Abstractions.Messaging;
using Interfaces.Gateways;
using Interfaces.Services;
using Constellation.Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using MimeKit;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RequestContactRemovalCommandHandler 
    : ICommandHandler<RequestContactRemovalCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailGateway _emailSender;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly ILogger _logger;

    public RequestContactRemovalCommandHandler(
        ISchoolContactRepository contactRepository,
        IEmailGateway emailSender,
        IRazorViewToStringRenderer razorService,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _emailSender = emailSender;
        _razorService = razorService;
        _logger = logger.ForContext<RequestContactRemovalCommand>();
    }
    
    public async Task<Result> Handle(RequestContactRemovalCommand request, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);
        SchoolContactRole role = contact.Assignments.FirstOrDefault(role => role.Id == request.RoleId);

        if (role is null)
        {
            _logger
                .ForContext(nameof(RequestContactRemovalCommand), request, true)
                .ForContext(nameof(Error), SchoolContactRoleErrors.NotFound(request.RoleId), true)
                .Warning("Failed to send request to remove school contact");

            return Result.Failure(SchoolContactRoleErrors.NotFound(request.RoleId));
        }

        // Send email to school requesting removal
        string viewModel = "<p>A school contact change has been requested:</p>";
        viewModel += $"<p><strong>{contact.DisplayName}</strong> should be removed as <strong>{role.Role}</strong> at <strong>{role.SchoolName}</strong></p>";
        viewModel += $"<p>This change was requested by <strong>{request.CancelledBy}</strong> on <strong>{request.CancelledAt}</strong> with the comment:</p>";
        viewModel += $"<p><strong>{request.Comment}<strong></p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        Dictionary<string, string> toRecipients = new()
        {
            { "Aurora College IT Support", "support@aurora.nsw.edu.au" },
            { "Aurora College", "auroracoll-h.school@det.nsw.edu.au" }
        };

        MimeMessage response = await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", "School Contact change requested", body, cancellationToken);

        if (response is not null)
            return Result.Success();

        return Result.Failure(new("Gateway.Email", "Email sending failed"));
    }
}