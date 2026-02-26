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
public class MandatoryTrainingModel : BasePageModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public MandatoryTrainingModel(
        IAppSettingsService appSettings,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _logger = logger
            .ForContext<MandatoryTrainingModel>();
    }

    [ViewData]
    public string ActivePage => Models.ActivePage.Configuration;

    [BindProperty] public Dictionary<StaffId, List<Grade>> Contacts { get; set; } = [];

    public List<StaffSelectionListResponse> StaffMembers { get; set; } = [];

    public async Task OnGet()
    {
        MandatoryTrainingConfiguration? configuration = await _appSettings.MandatoryTraining();

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

        MandatoryTrainingConfiguration configuration = new(contacts.Value);

        await _appSettings.MandatoryTraining(configuration);

        return RedirectToPage("/Configuration/Index", new { area = "Admin" });
    }
}