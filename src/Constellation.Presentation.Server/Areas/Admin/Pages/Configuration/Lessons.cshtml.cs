namespace Constellation.Presentation.Server.Areas.Admin.Pages.Configuration;

using Application.Domains.AppSettings.Models;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.AppSettings.Queries.BuildStaffDictionary;
using Constellation.Application.Domains.EmergencyConsole.Queries.GetContactDetails;
using Constellation.Application.Domains.StaffMembers.Models;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Constellation.Core.Enums;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Configuration_Edit_Value)]
public class LessonsModel : BasePageModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public LessonsModel(
        IAppSettingsService appSettings,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _logger = logger
            .ForContext<LessonsModel>();
    }

    [ViewData]
    public string ActivePage => Models.ActivePage.Configuration;

    [BindProperty] public string CoordinatorEmail { get; set; } = string.Empty;
    [BindProperty] public string CoordinatorName { get; set; } = string.Empty;
    [BindProperty] public string CoordinatorTitle { get; set; } = string.Empty;
    [BindProperty] public Dictionary<StaffId, List<Grade>> Contacts { get; set; } = [];
    
    public List<StaffSelectionListResponse> StaffMembers { get; set; } = [];

    public async Task OnGet()
    {
        LessonsConfiguration? configuration = await _appSettings.Lessons();

        if (configuration is not null)
        {
            CoordinatorEmail = configuration.CoordinatorEmail;
            CoordinatorName = configuration.CoordinatorName;
            CoordinatorTitle = configuration.CoordinatorTitle;
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
    }

    public async Task<IActionResult> OnPostSave()
    {
        Result<Dictionary<StaffMember, List<Grade>>> contacts = await _mediator.Send(new BuildStaffDictionaryQuery(Contacts));

        if (contacts.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(contacts.Error);

            Result<List<StaffSelectionListResponse>> staffMembers = await _mediator.Send(new GetStaffForSelectionListQuery());
            StaffMembers = staffMembers.Value;

            return Page();
        }

        LessonsConfiguration configuration = new(
            CoordinatorName ?? string.Empty,
            CoordinatorTitle ?? string.Empty,
            CoordinatorEmail ?? string.Empty,
            contacts.Value);

        await _appSettings.Lessons(configuration);

        return RedirectToPage("/Configuration/Index", new { area = "Admin" });
    }
}