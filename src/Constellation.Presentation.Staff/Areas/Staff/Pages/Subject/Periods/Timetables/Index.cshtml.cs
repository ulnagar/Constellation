namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Periods.Timetables;

using Application.Common.PresentationModels;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Periods_Timetables;
    [ViewData] public string PageTitle => "Timetables";

    public List<StaffSelectionListResponse> Staff { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost(string staffId)
    {
        if (string.IsNullOrWhiteSpace(staffId))
        {
            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/Periods/Timetables/Staff", new { area = "Staff", Id = staffId });
    }

    private async Task PreparePage()
    {
        _logger
            .Information("Requested to retrieve list of staff for Timetable view by user {User}", _currentUserService.UserName);

        Result<List<StaffSelectionListResponse>> staffRequest = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staffRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), staffRequest.Error, true)
                .Warning("Failed to retrieve list of staff for Timetable view by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                staffRequest.Error,
                _linkGenerator.GetPathByPage("Dashboard", values: new { area = "Staff" }));

            return;
        }

        Staff = staffRequest.Value.OrderBy(staff => staff.LastName).ThenBy(staff => staff.FirstName).ToList();
    }
}