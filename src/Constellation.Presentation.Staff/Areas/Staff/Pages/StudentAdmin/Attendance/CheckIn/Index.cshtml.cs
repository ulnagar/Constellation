namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.CheckIn;

using Application.Common.PresentationModels;
using Application.Domains.Attendance.CheckIns.Queries.GetCheckInResponses;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Attendance.Checkin;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.StudentAdmin_AttendanceList_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<IndexModel>();
    }

    [ViewData]
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_CheckIn;

    [ViewData]
    public string PageTitle => "Check In Data";

    public List<CheckInResponse> Responses { get; set; } = [];


    public async Task OnGet()
    {
        Result<List<CheckInResponse>> responses = await _mediator.Send(new GetCheckInResponsesQuery());

        if (responses.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(responses.Error);

            return;
        }

        Responses = responses.Value;
    }
}