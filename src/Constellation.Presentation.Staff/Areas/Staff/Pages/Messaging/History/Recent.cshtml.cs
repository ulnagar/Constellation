namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Messaging.History;

using Application.Domains.Messaging.History.Models;
using Application.Domains.Messaging.History.Queries.GetRecentCommunicationsHistory;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using System.Text;
using System.Text.Encodings.Web;

[HasPermission(AuthPermission.Messaging_View_Value)]
public class RecentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public RecentModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _logger = logger
            .ForContext<RecentModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Messaging_History_List;
    [ViewData] public string PageTitle => "Messaging History";

    public List<CommunicationRecordResponse> Records { get; set; } = [];

    public async Task OnGet() { }

    public async Task<JsonResult> OnGetData(
        int draw,
        int start,
        int length,
        string? searchValue,
        MessagingHistoryDateRange dateRange,
        CancellationToken cancellationToken = default)
    {
        int pageNumber = (start / length) + 1;
        int pageSize = length;

        var result = await _mediator.Send(
            new GetRecentCommunicationsHistoryQuery(searchValue, dateRange, pageNumber, pageSize),
            cancellationToken);

        if (result.IsFailure)
        {
            return new JsonResult(new
            {
                draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = Array.Empty<object>(),
                error = result.Error.Message
            });
        }

        var data = result.Value.Items.Select(message => new
        {
            type = message.Type.Value,
            typeIcon = message.Type == MessageType.Email ? "fal fa-envelope" : "fal fa-comment-alt-lines",
            timestamp = message.Timestamp.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss tt"),
            fromName = message.From.Name,
            fromContact = message.From.Contact,
            recipientsHtml = BuildRecipientsHtml(message),
            subject = message.Subject,
            status = message.Status.Name,
            viewUrl = Url.Page("/Messaging/History/ViewMessage", new { area = "Staff", id = message.Id, type = message.Type.Value })
        });

        return new JsonResult(new
        {
            draw,
            recordsTotal = result.Value.TotalCount,
            recordsFiltered = result.Value.TotalCount,
            data
        });
    }

    private static string BuildRecipientsHtml(CommunicationRecordResponse message)
    {
        StringBuilder sb = new();

        sb.AppendLine("""<span class="font-weight-bold">To:</span>""");
        foreach (var entry in message.Recipients.Where(entry => entry.Type == EmailRecipientType.To).OrderBy(entry => entry.Name))
        {
            sb.AppendLine($"""<span title="{HtmlEncoder.Default.Encode(entry.Contact)}" role="button" class="badge border border-secondary text-secondary text-nowrap">{HtmlEncoder.Default.Encode(entry.Name)}</span><br/>""");
        }

        if (message.Recipients.Any(entry => entry.Type == EmailRecipientType.Cc))
        {
            sb.AppendLine("""<span class="font-weight-bold">Cc:</span>""");

            foreach (var entry in message.Recipients.Where(entry => entry.Type == EmailRecipientType.Cc).OrderBy(entry => entry.Name))
            {
                sb.AppendLine($"""<span title="{HtmlEncoder.Default.Encode(entry.Contact)}" role="button" class="badge border border-secondary text-secondary text-nowrap">{HtmlEncoder.Default.Encode(entry.Name)}</span><br/>""");
            }
        }

        if (message.Recipients.Any(entry => entry.Type == EmailRecipientType.Bcc))
        {
            sb.AppendLine("""<span class="font-weight-bold">Bcc:</span>""");

            foreach (var entry in message.Recipients.Where(entry => entry.Type == EmailRecipientType.Bcc).OrderBy(entry => entry.Name))
            {
                sb.AppendLine($"""<span title="{HtmlEncoder.Default.Encode(entry.Contact)}" role="button" class="badge border border-secondary text-secondary text-nowrap">{HtmlEncoder.Default.Encode(entry.Name)}</span><br/>""");
            }
        }

        return sb.ToString();
    }
}