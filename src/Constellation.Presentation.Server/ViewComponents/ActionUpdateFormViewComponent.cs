namespace Constellation.Presentation.Server.ViewComponents;

using Application.Families.GetFamilyContactsForStudent;
using Application.Families.Models;
using Core.Abstractions.Clock;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.ActionUpdateForm;

public class ActionUpdateFormViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    private readonly ICaseRepository _caseRepository;
    private readonly IDateTimeProvider _dateTime;

    public ActionUpdateFormViewComponent(
        ISender mediator,
        ICaseRepository caseRepository,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _caseRepository = caseRepository;
        _dateTime = dateTime;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid caseId, Guid actionId)
    {
        Case item = await _caseRepository.GetById(CaseId.FromValue(caseId));

        if (item is null)
            return Content(string.Empty);

        Action action = item.Actions.FirstOrDefault(action => action.Id == ActionId.FromValue(actionId));

        if (action is null)
            return Content(string.Empty);

        string studentId = item.Type!.Equals(CaseType.Attendance) ?
            ((AttendanceCaseDetail)item.Detail)!.StudentId :
            null;

        // Limit the datetime-local field precision to minutes 
        DateTime now = _dateTime.Now;
        now = now.AddSeconds(-(now.Second));
        now = now.AddMilliseconds(-(now.Millisecond));

        switch (action)
        {
            case SendEmailAction emailAction:
                break;

            case CreateSentralEntryAction sentralAction:
                CreateSentralEntryActionViewModel sentralViewModel = new();

                if (item.Type!.Equals(CaseType.Attendance) && ((AttendanceCaseDetail)item.Detail)!.Severity.Equals(AttendanceSeverity.BandTwo))
                    sentralViewModel.NotRequiredAllowed = true;

                return View("CreateSentralEntryAction", sentralViewModel);

            case ConfirmSentralEntryAction confirmAction:
                ConfirmSentralEntryActionViewModel confirmViewModel = new();

                return View("ConfirmSentralEntryAction", confirmViewModel);

            case PhoneParentAction phoneAction:
                if (string.IsNullOrWhiteSpace(studentId))
                    return Content(string.Empty);

                Result<List<FamilyContactResponse>> phoneParentRequest = await _mediator.Send(new GetFamilyContactsForStudentQuery(studentId));

                if (phoneParentRequest.IsFailure)
                    return Content(string.Empty);

                PhoneParentActionViewModel phoneViewModel = new()
                {
                    Parents = phoneParentRequest.Value,
                    DateOccurred = now
                };

                return View("PhoneParentAction", phoneViewModel);

            case ParentInterviewAction interviewAction:
                if (string.IsNullOrWhiteSpace(studentId))
                    return Content(string.Empty);

                Result<List<FamilyContactResponse>> interviewParentRequest = await _mediator.Send(new GetFamilyContactsForStudentQuery(studentId));

                if (interviewParentRequest.IsFailure)
                    return Content(string.Empty);

                ParentInterviewActionViewModel interviewViewModel = new()
                {
                    ActionId = action.Id,
                    Parents = interviewParentRequest.Value,
                    DateOccurred = now
                };

                interviewViewModel.Attendees.Add(InterviewAttendee.Create(new(), "Brett McDonald", "Parent"));
                interviewViewModel.Attendees.Add(InterviewAttendee.Create(new(), "Joan McDonald", "Parent"));

                return View("ParentInterviewAction", interviewViewModel);

            default:
                return Content(string.Empty);
        };

        return Content(string.Empty);
    }
}