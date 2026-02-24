namespace Constellation.Presentation.Server.Areas.Admin.Pages.Configuration;

using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Constellation.Core.Enums;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.StaffMembers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Configuration_Edit_Value)]
public class AbsencesModel : BasePageModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public AbsencesModel(
        IAppSettingsService appSettings,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _logger = logger
            .ForContext<AbsencesModel>();
    }

    [ViewData]
    public string ActivePage => Models.ActivePage.Configuration;

    [BindProperty]
    public int PartialLengthThreshold { get; set; }
    [BindProperty]
    public string ContactName { get; set; } = string.Empty;
    [BindProperty]
    public string ContactTitle { get; set; } = string.Empty;
    [BindProperty]
    public string ContactEmail { get; set; } = string.Empty;

    [BindProperty]
    public List<AbsenceReason> DiscountedWholeReasons { get; set; } = [];
    [BindProperty]
    public List<AbsenceReason> DiscountedPartialReasons { get; set; } = [];
    [BindProperty]
    public Dictionary<StaffMember, List<Grade>> RollMarkingReportRecipients { get; }

    public List<AbsenceReason> AbsenceReasons { get; set; } = [];
    public List<StaffSelectionListResponse> StaffMembers { get; set; } = [];

    public async Task OnGet()
    {
        var configuration = await _appSettings.Absences();

        if (configuration is null)
        {
            return;
        }

        PartialLengthThreshold = configuration.PartialLengthThreshold;
        ContactName = configuration.ContactName;
        ContactTitle = configuration.ContactTitle;
        ContactEmail = configuration.ContactEmail;
        DiscountedWholeReasons = configuration.DiscountedWholeReasons.ToList();
        DiscountedPartialReasons = configuration.DiscountedPartialReasons.ToList();


        AbsenceReasons = AbsenceReason.GetEnumerable.ToList();
        Result<List<StaffSelectionListResponse>> staffMembers = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staffMembers.IsFailure)
        {
            return;
        }

        StaffMembers = staffMembers.Value;
    }
}