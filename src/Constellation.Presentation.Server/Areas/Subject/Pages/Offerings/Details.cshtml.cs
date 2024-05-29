namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Application.Enrolments.EnrolStudent;
using Constellation.Application.Enrolments.UnenrolStudent;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.AddSessionToOffering;
using Constellation.Application.Offerings.AddTeacherToOffering;
using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.Offerings.RemoveAllSessions;
using Constellation.Application.Offerings.RemoveResourceFromOffering;
using Constellation.Application.Offerings.RemoveSession;
using Constellation.Application.Offerings.RemoveTeacherFromOffering;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveAllSessionsModal;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveResourceFromOfferingModal;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveSessionModal;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveTeacherFromOfferingModal;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.UnenrolStudentModal;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Pages.Shared.Components.AddSessionToOffering;
using Presentation.Shared.Pages.Shared.Components.AddTeacherToOffering;
using Presentation.Shared.Pages.Shared.Components.EnrolStudentInOffering;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _sender;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender sender,
        LinkGenerator linkGenerator)
    {
        _sender = sender;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => SubjectPages.Offerings;
    
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public OfferingDetailsResponse Offering { get; set; }

    public async Task OnGet()
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<OfferingDetailsResponse> query = await _sender.Send(new GetOfferingDetailsQuery(offeringId));

        if (query.IsFailure)
        {
            Error = new()
            {
                Error = query.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Index", values: new { area = "Subject" })
            };

            return;
        }

        Offering = query.Value;
    }

    public IActionResult OnPostAjaxUnenrolConfirmation(
        string studentId,
        string studentName,
        string courseName,
        string offeringName)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        UnenrolStudentModalViewModel viewModel = new(
            offeringId,
            studentId,
            studentName,
            courseName,
            offeringName);

        return Partial("UnenrolStudentModal", viewModel);
    }

    public async Task<IActionResult> OnGetUnenrolStudent(
        string studentId)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result request = await _sender.Send(new UnenrolStudentCommand(studentId, offeringId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxRemoveSession(
        string sessionPeriod,
        Guid sessionId,
        string courseName,
        string offeringName)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);
        SessionId SessionId = SessionId.FromValue(sessionId);

        RemoveSessionModalViewModel viewModel = new(
            offeringId,
            SessionId,
            sessionPeriod,
            courseName,
            offeringName);

        return Partial("RemoveSessionModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveSession(
        Guid sessionId)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);
        SessionId SessionId = SessionId.FromValue(sessionId);

        Result request = await _sender.Send(new RemoveSessionCommand(offeringId, SessionId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxRemoveAllSessions(
        string courseName,
        string offeringName)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        RemoveAllSessionsModalViewModel viewModel = new(
            offeringId,
            courseName,
            offeringName);

        return Partial("RemoveAllSessionsModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveAllSessions()
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result request = await _sender.Send(new RemoveAllSessionsCommand(offeringId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddTeacher(
        AddTeacherToOfferingSelection viewModel)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        AssignmentType type = AssignmentType.FromValue(viewModel.AssignmentType);

        Result request = await _sender.Send(new AddTeacherToOfferingCommand(offeringId, viewModel.StaffId, type));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEnrolStudent(
        EnrolStudentInOfferingSelection viewModel)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result request = await _sender.Send(new EnrolStudentCommand(viewModel.StudentId, offeringId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxRemoveTeacher(
        string staffId,
        string teacherName,
        string assignmentType,
        string courseName,
        string offeringName)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        RemoveTeacherFromOfferingModalViewModel viewModel = new(
            offeringId,
            staffId,
            teacherName,
            assignmentType,
            courseName,
            offeringName);

        return Partial("RemoveTeacherFromOfferingModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveTeacher(
        string staffId,
        string assignmentType)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);
        AssignmentType teacherAssignmentType = AssignmentType.FromValue(assignmentType);

        Result request = await _sender.Send(new RemoveTeacherFromOfferingCommand(offeringId, staffId, teacherAssignmentType));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxRemoveResource(
        string courseName,
        string offeringName,
        string resourceName,
        string resourceType,
        Guid resourceId)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);
        ResourceId id = ResourceId.FromValue(resourceId);

        RemoveResourceFromOfferingModalViewModel viewModel = new(
            offeringId,
            id,
            resourceName,
            resourceType,
            courseName,
            offeringName);

        return Partial("RemoveResourceFromOfferingModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveResource(
        Guid resourceId)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);
        ResourceId id = ResourceId.FromValue(resourceId);

        Result request = await _sender.Send(new RemoveResourceFromOfferingCommand(offeringId, id));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddSession(
        AddSessionToOfferingSelection viewModel)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result request = await _sender.Send(new AddSessionToOfferingCommand(offeringId, viewModel.PeriodId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }
}
