namespace Constellation.Presentation.Server.Areas.Admin.Pages.Configuration;

using Application.Domains.AppSettings.Models;
using Application.Domains.AppSettings.Queries.BuildStaffDictionary;
using Application.Domains.StaffMembers.Models;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Constellation.Core.Enums;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Configuration_Edit_Value)]
public class CanvasModel : BasePageModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public CanvasModel(
        IAppSettingsService appSettings,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _logger = logger
            .ForContext<CanvasModel>();
    }

    [ViewData]
    public string ActivePage => Models.ActivePage.Configuration;

    [BindProperty] public bool UseGroups { get; set; }
    [BindProperty] public bool UseSections { get; set; }
    [BindProperty] public Dictionary<StaffId, List<Grade>> Admins { get; set; } = [];

    public List<StaffSelectionListResponse> StaffMembers { get; set; } = [];

    public async Task OnGet()
    {
        CanvasConfiguration? configuration = await _appSettings.Canvas();

        if (configuration is not null)
        {
            UseGroups = configuration.UseGroups;
            UseSections = configuration.UseSections;
            Admins = configuration.Admins
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
        Result<Dictionary<StaffMember, List<Grade>>> admins = await _mediator.Send(new BuildStaffDictionaryQuery(Admins));

        if (admins.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(admins.Error);

            Result<List<StaffSelectionListResponse>> staffMembers = await _mediator.Send(new GetStaffForSelectionListQuery());
            StaffMembers = staffMembers.Value;

            return Page();
        }

        CanvasConfiguration configuration = new(
            UseGroups,
            UseSections,
            admins.Value);

        await _appSettings.Canvas(configuration);

        return RedirectToPage("/Configuration/Index", new { area = "Admin" });
    }
}