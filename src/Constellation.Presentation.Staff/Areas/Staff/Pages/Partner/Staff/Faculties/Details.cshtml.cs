namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Application.Domains.StaffMembers.Commands.RemoveStaffFromFaculty;
using Constellation.Application.Domains.Faculties.Queries.GetFacultyDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Faculties.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanViewFacultyDetails)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;
    [ViewData] public string PageTitle { get; set; } = "Faculty Details";

    [BindProperty(SupportsGet = true)]
    public Guid FacultyId { get; set; }

    public FacultyDetailsResponse Faculty { get; set; }

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve details of Faculty with id {FacultyId} by user {User}", FacultyId, _currentUserService.UserName);

        FacultyId facultyId = Core.Models.Faculties.Identifiers.FacultyId.FromValue(FacultyId);

        Result<FacultyDetailsResponse> facultyRequest = await _mediator.Send(new GetFacultyDetailsQuery(facultyId));

        if (facultyRequest.IsSuccess)
            Faculty = facultyRequest.Value;
    }

    public async Task<IActionResult> OnPostRemoveMember([FromQuery]string staffId)
    {
        FacultyId facultyId = Core.Models.Faculties.Identifiers.FacultyId.FromValue(FacultyId);

        RemoveStaffFromFacultyCommand command = new(staffId, facultyId);

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
