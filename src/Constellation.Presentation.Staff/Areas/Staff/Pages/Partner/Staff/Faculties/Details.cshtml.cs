namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Application.Domains.StaffMembers.Commands.RemoveStaffFromFaculty;
using Application.Domains.StaffMembers.Queries.GetStaffMemberNameById;
using Constellation.Application.Domains.Faculties.Queries.GetFacultyDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Faculties.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Extensions;
using Serilog;
using Shared.PartialViews.ConfirmRemoveStaffFromFacultyModal;

[HasPermission(AuthPermission.Partners_Faculties_View_Value)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;
    [ViewData] public string PageTitle { get; set; } = "Faculty Details";

    [BindProperty(SupportsGet = true)]
    public FacultyId FacultyId { get; set; }

    public FacultyDetailsResponse Faculty { get; set; }

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve details of Faculty with id {FacultyId} by user {User}", FacultyId, _currentUserService.UserName);

        Result<FacultyDetailsResponse> facultyRequest = await _mediator.Send(new GetFacultyDetailsQuery(FacultyId));

        if (facultyRequest.IsSuccess)
            Faculty = facultyRequest.Value;
    }

    public async Task<IActionResult> OnPostAjaxRemoveMember(StaffId staffId)
    {
        Result<string> staffName = await _mediator.Send(new GetStaffMemberNameByIdQuery(staffId));

        if (staffName.IsFailure)
            return BadRequest();

        ConfirmRemoveStaffFromFacultyModalViewModel viewModel = new()
        {
            StaffName = staffName.Value, 
            StaffId = staffId
        };

        return Partial("ConfirmRemoveStaffFromFacultyModal", viewModel);
    }

    public async Task<IActionResult> OnPostRemoveMember(StaffId staffId)
    {
        RemoveStaffFromFacultyCommand command = new(staffId, FacultyId);

        _logger
            .ForContext(nameof(RemoveStaffFromFacultyCommand), command, true)
            .Information("Requested to remove member from Faculty with id {FacultyId} by user {User}", FacultyId, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove member from Faculty with id {FacultyId} by user {User}", FacultyId, _currentUserService.UserName);

        return RedirectToPage("/Partner/Staff/Faculties/Details", routeValues: new { FacultyId, area = "Staff" });
    }
}
