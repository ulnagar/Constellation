namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ScheduleTutorialRequest;

using Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;
using Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Application.Domains.Tutorials.Requests.Queries.GetProposedTutorialNameForRequest;
using Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestById;
using Core.Abstractions.Clock;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public sealed class ScheduleTutorialRequestViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _dateTime;

    public ScheduleTutorialRequestViewComponent(
        ISender mediator,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _dateTime = dateTime;
    }

    public async Task<IViewComponentResult> InvokeAsync(RequestId requestId)
    {
        Result<TutorialRequestDetailsResponse> request = await _mediator.Send(new GetTutorialRequestByIdQuery(requestId));

        if (request.IsFailure)
            return Content(string.Empty);

        ScheduleTutorialRequestSelection viewModel = new();

        foreach (var period in request.Value.Periods)
        {
            viewModel.Periods.Add(new(
                period.Id,
                period.ToString()));
        }

        Result<List<StaffSelectionListResponse>> staffMembers = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staffMembers.IsFailure)
            return Content(string.Empty);

        viewModel.StaffMembers = staffMembers.Value;

        Result<List<ValidAttendenceReportDate>> validDates = await _mediator.Send(new GetTermsAndWeeksForCurrentYearQuery());

        if (validDates.IsFailure)
            return Content(string.Empty);

        viewModel.ValidStartDates = validDates.Value.Where(entry => entry.StartDate > _dateTime.Now).ToList();

        Result<TutorialName> proposedName = await _mediator.Send(new GetProposedTutorialNameForRequestQuery(requestId));

        if (proposedName.IsFailure)
            return Content(string.Empty);

        viewModel.Name = proposedName.Value;

        return View(viewModel);
    }
}
