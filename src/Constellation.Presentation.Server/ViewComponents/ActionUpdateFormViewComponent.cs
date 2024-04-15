namespace Constellation.Presentation.Server.ViewComponents;

using Application.Families.GetFamilyContactsForStudent;
using Application.Families.Models;
using Application.Parents.GetParentWithStudentIds;
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
                string studentId = item.Type!.Equals(CaseType.Attendance) ?
                    ((AttendanceCaseDetail)item.Detail)!.StudentId :
                    null;

                if (string.IsNullOrWhiteSpace(studentId))
                    return Content(string.Empty);

                Result<List<FamilyContactResponse>> parents = await _mediator.Send(new GetFamilyContactsForStudentQuery(studentId));

                if (parents.IsFailure)
                    return Content(string.Empty);

                // Limit the datetime-local field precision to minutes 
                DateTime now = _dateTime.Now;
                now = now.AddSeconds(-(now.Second));
                now = now.AddMilliseconds(-(now.Millisecond));

                PhoneParentActionViewModel phoneViewModel = new()
                {
                    Parents = parents.Value,
                    DateOccurred = now
                };

                return View("PhoneParentAction", phoneViewModel);

            case ParentInterviewAction interviewAction:
                break;

            default:
                return Content(string.Empty);
        };

        return Content(string.Empty);
    }
}