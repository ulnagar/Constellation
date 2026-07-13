namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Messaging.History;

using Application.Domains.Messaging.History.Queries.GetCommunicationsHistoryForContact;
using Application.Models.Auth;
using Constellation.Application.Domains.Messaging.History.Models;
using Constellation.Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Primitives;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using System.Globalization;
using System.Reflection;

[HasPermission(AuthPermission.Messaging_View_Value)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public DetailsModel(
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
            .ForContext<DetailsModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Messaging_History_List;
    [ViewData] public string PageTitle => "Messaging History";


    [BindProperty(SupportsGet = true)]
    public string Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public List<CommunicationRecordResponse> Records { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        if (!IdTypes.TryGetValue(Type, out var entry))
            return BadRequest($"Unknown entity type: {Type}");

        try
        {
            var convertedValue = ConvertValue(Id, entry.ValueType);
            var id = (IStronglyTypedId)entry.FromValue.Invoke(null, new[] { convertedValue });

            Result<List<CommunicationRecordResponse>> responses = await _mediator.Send(new GetCommunicationsHistoryForContactQuery(id));

            if (responses.IsFailure)
            {
                return Page();
            }

            Records = responses.Value;
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
            { "FamilyId", Build<FamilyId, Guid>() },
            { "ParentId",   Build<ParentId, Guid>() },
            { "SchoolCode",    Build<SchoolCode, string>() },
            { "SchoolContactId",   Build<SchoolContactId, Guid>() },
            { "StaffId",   Build<StaffId, Guid>() },
            { "StudentId",   Build<StudentId, Guid>() }
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