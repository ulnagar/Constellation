namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Application.StaffMembers.GetStaffById;
using Application.Timetables.GetStaffIntegratedTimetableData;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class TimetableModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public TimetableModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<TimetableModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Staff;
    [ViewData] public string PageTitle { get; set; } = "Staff Timetable";

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    public List<StaffIntegratedTimetableResponse> TimetableData { get; set; } = [];

    public Name StaffName { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve timetable details of Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(Id));

        if (staffMember.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                staffMember.Error,
                _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), staffMember.Error, true)
                .Warning("Failed to retrieve timetable details of Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        StaffName = staffMember.Value.Name;

        Result<List<StaffIntegratedTimetableResponse>> timetableRequest = await _mediator.Send(new GetStaffIntegratedTimetableDataQuery(Id));

        if (timetableRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                timetableRequest.Error,
                _linkGenerator.GetPathByPage("/Partner/Staff/Details", values: new { area = "Staff", Id }));

            _logger
                .ForContext(nameof(Error), timetableRequest.Error, true)
                .Warning("Failed to retrieve timetable details of Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        TimetableData = timetableRequest.Value.Where(period => period.Duration > 0).ToList();
    }
}