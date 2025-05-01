namespace Constellation.Application.Domains.ExternalSystems.HelpDesk.Commands.SubmitSupportTicket;

using Abstractions.Messaging;
using Core.Shared;
using Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SubmitSupportTicketCommandHandler : ICommandHandler<SubmitSupportTicketCommand>
{
    private readonly IEmailService _emailService;

    public SubmitSupportTicketCommandHandler(
        IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<Result> Handle(SubmitSupportTicketCommand request, CancellationToken cancellationToken)
    {
        string subject = $"Student Portal Support Request - {request.IssueType} {request.DeviceIdentifier}";

        string body = $"Device: {request.DeviceIdentifier}<br />Issue:<br />{request.Description}";

        await _emailService.SendSupportTicketRequest(request.Submitter, subject, body, cancellationToken);

        return Result.Success();
    }
}