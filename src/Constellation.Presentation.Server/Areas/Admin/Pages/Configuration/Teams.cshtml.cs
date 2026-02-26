namespace Constellation.Presentation.Server.Areas.Admin.Pages.Configuration;

using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.AppSettings.Models;
using Constellation.Application.Domains.AppSettings.Queries.BuildStaffDictionary;
using Constellation.Application.Domains.StaffMembers.Models;
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
public class TeamsModel : BasePageModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public TeamsModel(
        IAppSettingsService appSettings,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _logger = logger
            .ForContext<TeamsModel>();
    }

    [ViewData]
    public string ActivePage => Models.ActivePage.Configuration;

    [BindProperty] public Dictionary<StaffId, List<Grade>> MandatoryOwners { get; set; } = [];
    [BindProperty] public Dictionary<StaffId, List<Grade>> StudentTeamOwners { get; set; } = [];
    [BindProperty] public Dictionary<StaffId, List<Grade>> StudentChannelOwners { get; set; } = [];

    public List<StaffSelectionListResponse> StaffMembers { get; set; } = [];

    public async Task OnGet()
    {
        TeamsConfiguration? configuration = await _appSettings.Teams();

        if (configuration is not null)
        {
            MandatoryOwners = configuration.MandatoryOwners
                .OrderBy(entry => entry.Key.Name.SortOrder)
                .ToDictionary(item => item.Key.Id, item => item.Value);
            StudentTeamOwners = configuration.StudentTeamOwners
                .OrderBy(entry => entry.Key.Name.SortOrder)
                .ToDictionary(item => item.Key.Id, item => item.Value);
            StudentChannelOwners = configuration.StudentChannelOwners
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
        Result<Dictionary<StaffMember, List<Grade>>> mandatoryOwners = await _mediator.Send(new BuildStaffDictionaryQuery(MandatoryOwners));
        Result<Dictionary<StaffMember, List<Grade>>> studentTeamOwners = await _mediator.Send(new BuildStaffDictionaryQuery(StudentTeamOwners));
        Result<Dictionary<StaffMember, List<Grade>>> studentChannelOwners = await _mediator.Send(new BuildStaffDictionaryQuery(StudentChannelOwners));

        if (mandatoryOwners.IsFailure || studentTeamOwners.IsFailure || studentChannelOwners.IsFailure)
        {
            if (mandatoryOwners.IsFailure)
                ModalContent = ErrorDisplay.Create(mandatoryOwners.Error);

            if (studentTeamOwners.IsFailure)
                ModalContent = ErrorDisplay.Create(studentTeamOwners.Error);

            if (studentChannelOwners.IsFailure)
                ModalContent = ErrorDisplay.Create(studentTeamOwners.Error);

            Result<List<StaffSelectionListResponse>> staffMembers = await _mediator.Send(new GetStaffForSelectionListQuery());
            StaffMembers = staffMembers.Value;

            return Page();
        }

        TeamsConfiguration configuration = new(
            mandatoryOwners.Value,
            studentTeamOwners.Value,
            studentChannelOwners.Value);

        await _appSettings.Teams(configuration);

        return RedirectToPage("/Configuration/Index", new { area = "Admin" });
    }
}