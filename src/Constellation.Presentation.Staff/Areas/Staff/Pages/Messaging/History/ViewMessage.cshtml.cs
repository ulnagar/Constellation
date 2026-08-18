namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Messaging.History;

using Application.Domains.Messaging.History.Queries.GetMessageDetails;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Messaging.Email.Identifiers;
using Core.Models.Messaging.Sms.Identifiers;
using Core.Primitives;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using System.Globalization;
using System.Reflection;

[HasPermission(AuthPermission.Messaging_View_Value)]
public class ViewMessageModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ViewMessageModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ViewMessageModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Messaging_History_List;
    [ViewData] public string PageTitle => "Messaging History";

    [BindProperty(SupportsGet = true)]
    public string Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public MessageDetailResponse? Message { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (!IdTypes.TryGetValue(Type, out var entry))
            return BadRequest($"Unknown entity type: {Type}");

        try
        {
            object convertedValue = ConvertValue(Id, entry.ValueType);
            IStronglyTypedId? id = (IStronglyTypedId)entry.FromValue.Invoke(null, new[] { convertedValue });

            if (id is null)
                return BadRequest($"Unknown entity id: {convertedValue}");

            GetMessageDetailsQuery query = new(id);

            Result<MessageDetailResponse> message = await _mediator.Send(query);

            if (message.IsFailure)
            {
                _logger
                    .ForContext(nameof(GetMessageDetailsQuery), query, true)
                    .ForContext(nameof(Error), message.Error, true)
                    .Warning("Failed to retrieve message details by user {User}", _currentUserService.UserName);
            }
            else
            {
                Message = message.Value;
            }
        }
        catch (FormatException)
        {
            return BadRequest($"Invalid id format for type: {Type}");
        }

        return Page();
    }

    private static readonly Dictionary<string, (Type IdType, Type ValueType, MethodInfo FromValue)> IdTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Email", Build<EmailId, Guid>() },
            { "Sms",   Build<SmsId, Guid>() }
        };

    private static (Type, Type, MethodInfo) Build<TId, TValue>() where TId : IStronglyTypedId
    {
        var fromValue = typeof(TId).GetMethod("FromValue", BindingFlags.Static | BindingFlags.Public)
                        ?? throw new InvalidOperationException($"{typeof(TId).Name} does not have a static FromValue method");

        return (typeof(TId), typeof(TValue), fromValue);
    }

    private static object ConvertValue(string value, Type targetType)
    {
        if (targetType == typeof(Guid))
        {
            if (!Guid.TryParse(value, out var guid))
                throw new FormatException($"'{value}' is not a valid GUID");

            return guid;
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
}