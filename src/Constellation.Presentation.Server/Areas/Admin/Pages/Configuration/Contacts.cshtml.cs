namespace Constellation.Presentation.Server.Areas.Admin.Pages.Configuration;

using Application.Domains.AppSettings.Models;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.AppSettings.Queries.BuildStaffDictionary;
using Constellation.Application.Domains.StaffMembers.Models;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Constellation.Core.Models.AppSettings.Enums;
using Constellation.Core.Shared;
using Core.Enums;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Configuration_Edit_Value)]
public class ContactsModel : BasePageModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public ContactsModel(
        IAppSettingsService appSettings,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _logger = logger
            .ForContext<ContactsModel>();
    }

    [ViewData] 
    public string ActivePage => Models.ActivePage.Configuration;

    [BindProperty(SupportsGet = true)] public ContactPosition? Position { get; set; }
    [BindProperty] public Dictionary<StaffId, List<Grade>> Contacts { get; set; } = [];

    public List<ContactPosition> Positions { get; set; } = [];
    public List<StaffSelectionListResponse> StaffMembers { get; set; } = [];

    public async Task OnGet()
    {
        Position ??= ContactPosition.CareersAdvisor;

        ContactsConfiguration? configuration = await _appSettings.Contacts(Position);

        if (configuration is not null)
        {
            Contacts = configuration.Contacts
                .OrderBy(entry => entry.Key.Name.SortOrder)
                .ToDictionary(item => item.Key.Id, item => item.Value);
        }

        Result<List<StaffSelectionListResponse>> staffMembers = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staffMembers.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(staffMembers.Error);

            return;
        }

        StaffMembers = staffMembers.Value;
        Positions = ContactPosition.GetEnumerable.ToList();
    }

    public async Task<IActionResult> OnPostSave()
    {
        Result<Dictionary<StaffMember, List<Grade>>> contacts = await _mediator.Send(new BuildStaffDictionaryQuery(Contacts));

        if (contacts.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(contacts.Error);

            Result<List<StaffSelectionListResponse>> staffMembers = await _mediator.Send(new GetStaffForSelectionListQuery());
            StaffMembers = staffMembers.Value;
            Positions = ContactPosition.GetEnumerable.ToList();

            return Page();
        }

        ContactsConfiguration configuration = new(
            Position,
            contacts.Value);

        await _appSettings.Contacts(configuration);

        return RedirectToPage("/Configuration/Index", new { area = "Admin" });
    }
}