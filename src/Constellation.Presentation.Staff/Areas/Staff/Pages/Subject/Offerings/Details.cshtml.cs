namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Canvas.ExportCanvasRubricResults;
using Application.Common.PresentationModels;
using Application.DTOs;
using Constellation.Application.Assignments.GetAssignmentsFromCourse;
using Constellation.Application.Enrolments.EnrolStudent;
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
using Constellation.Presentation.Staff.Areas;
using Core.Models.Offerings.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Components.AddSessionToOffering;
using Shared.Components.AddTeacherToOffering;
using Shared.Components.EnrolStudentInOffering;
using Shared.PartialViews.RemoveAllSessionsModal;
using Shared.PartialViews.RemoveResourceFromOfferingModal;
using Shared.PartialViews.RemoveSessionModal;
using Shared.PartialViews.RemoveTeacherFromOfferingModal;
using Shared.PartialViews.UnenrolStudentModal;

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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;
    
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public OfferingDetailsResponse Offering { get; set; }
    public List<AssignmentFromCourseResponse> Assignments { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<OfferingDetailsResponse> query = await _sender.Send(new GetOfferingDetailsQuery(offeringId));

        if (query.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                query.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" }));

            return;
        }

        Offering = query.Value;

        Result<List<AssignmentFromCourseResponse>> assignments = await _sender.Send(new GetAssignmentsFromCourseQuery(Offering.CourseId));

        if (assignments.IsFailure)
        {
            ModalContent = new ErrorDisplay(assignments.Error);

            return;
        }

        Assignments = assignments.Value.OrderBy(entry => entry.DueDate).ToList();
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
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

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
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

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
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddTeacher(
        AddTeacherToOfferingSelection viewModel)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        AssignmentType? type = AssignmentType.FromValue(viewModel.AssignmentType);

        if (type is null)
        {
            ModalContent = new ErrorDisplay(new("Offering.TeacherAssignment.AssignmentType", "Invalid Assignment Type"));

            return Page();
        }

        Result request = await _sender.Send(new AddTeacherToOfferingCommand(offeringId, viewModel.StaffId, type));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

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
            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

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
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

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
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();
            
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
            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();
            
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadRubricResults(
        int assignmentId)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<FileDto> request = await _sender.Send(new ExportCanvasRubricResultsQuery(offeringId, assignmentId));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

            return Page();
        }

        return File(request.Value.FileData, request.Value.FileType, request.Value.FileName);
    }
}
