namespace Constellation.Presentation.Shared.ViewComponents;

using Application.Contacts.GetContactListForStudent;
using Application.Contacts.Models;
using Application.Families.GetFamilyContactsForStudent;
using Application.Families.Models;
using Application.StaffMembers.GetStaffByEmail;
using Application.StaffMembers.Models;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;

    public ActionUpdateFormViewComponent(
        ISender mediator,
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
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

        if (action.Status.Equals(ActionStatus.Completed))
            return View("ActionCompleted", string.Empty);
        
        if (action.Status.Equals(ActionStatus.Cancelled))
            return View("ActionCancelled", string.Empty);

        string? studentId = 
            item.Type!.Equals(CaseType.Attendance) ? ((AttendanceCaseDetail)item.Detail!).StudentId 
            : item.Type!.Equals(CaseType.Compliance) ? ((ComplianceCaseDetail)item.Detail!).StudentId
            : null;

        // Limit the datetime-local field precision to minutes 
        DateTime now = _dateTime.Now;
        now = now.AddSeconds(-(now.Second));
        now = now.AddMilliseconds(-(now.Millisecond));

        switch (action)
        {
            case SendEmailAction emailAction:
                SendEmailActionViewModel emailViewModel = new();

                Result<List<ContactResponse>> contacts = await _mediator.Send(new GetContactListForStudentQuery(studentId));

                if (contacts.IsFailure)
                    return Content(string.Empty);

                foreach (ContactResponse contact in contacts.Value)
                {
                    SendEmailActionViewModel.ContactType type = contact.Category switch
                    {
                        not null when contact.Category.Equals(ContactCategory.Student) => SendEmailActionViewModel.ContactType.Student,
                        not null when contact.Category.Equals(ContactCategory.ResidentialFamily) => SendEmailActionViewModel.ContactType.ResidentialFamily,
                        not null when contact.Category.Equals(ContactCategory.ResidentialFather) => SendEmailActionViewModel.ContactType.ResidentialFamily,
                        not null when contact.Category.Equals(ContactCategory.ResidentialMother) => SendEmailActionViewModel.ContactType.ResidentialFamily,
                        not null when contact.Category.Equals(ContactCategory.NonResidentialFamily) => SendEmailActionViewModel.ContactType.NonResidentialFamily,
                        not null when contact.Category.Equals(ContactCategory.NonResidentialParent) => SendEmailActionViewModel.ContactType.NonResidentialFamily,
                        not null when contact.Category.Equals(ContactCategory.PartnerSchoolSchool) => SendEmailActionViewModel.ContactType.PartnerSchool,
                        not null when contact.Category.Equals(ContactCategory.PartnerSchoolACC) => SendEmailActionViewModel.ContactType.PartnerSchool,
                        not null when contact.Category.Equals(ContactCategory.PartnerSchoolSPT) => SendEmailActionViewModel.ContactType.PartnerSchool,
                        not null when contact.Category.Equals(ContactCategory.PartnerSchoolPrincipal) => SendEmailActionViewModel.ContactType.PartnerSchool,
                        not null when contact.Category.Equals(ContactCategory.AuroraTeacher) => SendEmailActionViewModel.ContactType.AuroraCollege,
                        not null when contact.Category.Equals(ContactCategory.AuroraHeadTeacher) => SendEmailActionViewModel.ContactType.AuroraCollege,
                        _ => SendEmailActionViewModel.ContactType.AuroraCollege
                    };

                    emailViewModel.Contacts.Add(
                        new()
                        {
                            Name = contact.Contact,
                            Email = contact.ContactEmail.Email,
                            Type = type,
                            Notes = contact.Category?.Name
                        });
                }

                string staffEmail = _currentUserService.EmailAddress;

                Result<StaffSelectionListResponse> staffMember = await _mediator.Send(new GetStaffByEmailQuery(staffEmail));

                if (staffMember.IsFailure)
                    return Content(string.Empty);

                emailViewModel.Senders.Add(
                    new()
                    {
                        Name = staffMember.Value.DisplayName,
                        Email = staffEmail
                    });

                emailViewModel.Senders.Add(
                    new()
                    {
                        Name = "Aurora College",
                        Email = "auroracoll-h.school@det.nsw.edu.au"
                    });

                emailViewModel.SentAt = now;

                return View("SendEmailAction", emailViewModel);

            case CreateSentralEntryAction sentralAction:
                CreateSentralEntryActionViewModel sentralViewModel = new();

                if (item.Type!.Equals(CaseType.Attendance) && ((AttendanceCaseDetail)item.Detail!).Severity.Equals(AttendanceSeverity.BandTwo))
                    sentralViewModel.NotRequiredAllowed = true;

                return View("CreateSentralEntryAction", sentralViewModel);

            case ConfirmSentralEntryAction confirmAction:
                ConfirmSentralEntryActionViewModel confirmViewModel = new();

                confirmViewModel.Warning =
                    item.Type!.Equals(CaseType.Attendance) ? ConfirmSentralEntryActionViewModel.AttendanceWarning
                    : item.Type!.Equals(CaseType.Compliance) ? ConfirmSentralEntryActionViewModel.ComplianceWarning
                    : string.Empty;

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

                return View("ParentInterviewAction", interviewViewModel);

            case SentralIncidentStatusAction incidentAction:
                SentralIncidentStatusActionViewModel incidentViewModel = new();

                return View("SentralIncidentStatusAction", incidentViewModel);

            default:
                return Content(string.Empty);
        };

        return Content(string.Empty);
    }
}