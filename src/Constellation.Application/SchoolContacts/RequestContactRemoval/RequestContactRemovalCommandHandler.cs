namespace Constellation.Application.SchoolContacts.RequestContactRemoval;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Core.Models;
using Core.Shared;
using MimeKit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RequestContactRemovalCommandHandler 
    : ICommandHandler<RequestContactRemovalCommand>
{
    private readonly ISchoolContactRoleRepository _roleRepository;
    private readonly IEmailGateway _emailSender;
    private readonly IRazorViewToStringRenderer _razorService;

    public RequestContactRemovalCommandHandler(
        ISchoolContactRoleRepository roleRepository,
        IEmailGateway emailSender,
        IRazorViewToStringRenderer razorService)
    {
        _roleRepository = roleRepository;
        _emailSender = emailSender;
        _razorService = razorService;
    }
    
    public async Task<Result> Handle(RequestContactRemovalCommand request, CancellationToken cancellationToken)
    {
        SchoolContactRole role = await _roleRepository.GetWithContactById(request.AssignmentId, cancellationToken);

        // Send email to school requesting removal
        string viewModel = "<p>A school contact change has been requested:</p>";
        viewModel += $"<p><strong>{role.SchoolContact.DisplayName}</strong> should be removed as <strong>{role.Role}</strong> at <strong>{role.School.Name}</strong></p>";
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