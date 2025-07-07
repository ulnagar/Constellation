namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Periods.Timetables;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;
using Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffIntegratedTimetableData;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class StaffModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public StaffModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<StaffModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Periods_Timetables;
    [ViewData] public string PageTitle { get; set; } = "Staff Timetable";

    [BindProperty(SupportsGet = true)]
    public StaffId Id { get; set; } = StaffId.Empty;

    public List<StaffIntegratedTimetableResponse> TimetableData { get; set; } = [];

    public Name StaffName { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve timetable details of Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(Id));

        if (staffMember.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                staffMember.Error,
                _linkGenerator.GetPathByPage("/Subject/Period/Timetables/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), staffMember.Error, true)
                .Warning("Failed to retrieve timetable details of Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        StaffName = staffMember.Value.Name;

        Result<List<StaffIntegratedTimetableResponse>> timetableRequest = await _mediator.Send(new GetStaffIntegratedTimetableDataQuery(Id));

        if (timetableRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                timetableRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Period/Timetables/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), timetableRequest.Error, true)
                .Warning("Failed to retrieve timetable details of Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        TimetableData = timetableRequest.Value.Where(period => period.Duration > 0).ToList();
    }
}