namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Application.Domains.Assignments.Models;
using Application.Domains.Assignments.Queries.GetRubricAssignmentsFromCourse;
using Application.Domains.Enrolments.Commands.EnrolStudentInOffering;
using Application.Domains.Enrolments.Commands.UnenrolStudentFromOffering;
using Application.Domains.LinkedSystems.Canvas.Commands.ExportCanvasAssignmentComments;
using Application.Domains.LinkedSystems.Canvas.Commands.ExportCanvasRubricResults;
using Application.DTOs;
using Constellation.Application.Domains.Offerings.Commands.AddSessionToOffering;
using Constellation.Application.Domains.Offerings.Commands.AddTeacherToOffering;
using Constellation.Application.Domains.Offerings.Commands.RemoveAllSessions;
using Constellation.Application.Domains.Offerings.Commands.RemoveResourceFromOffering;
using Constellation.Application.Domains.Offerings.Commands.RemoveSession;
using Constellation.Application.Domains.Offerings.Commands.RemoveTeacherFromOffering;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Canvas.Models;
using Core.Models.Offerings.Errors;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Timetables.Errors;
using Core.Models.Timetables.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender sender,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _sender = sender;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;
    [ViewData] public string PageTitle { get; set; } = "Offering Details";
    
    [BindProperty(SupportsGet = true)]
    public OfferingId Id { get; set; } = OfferingId.Empty;

    public OfferingDetailsResponse Offering { get; set; }
    public List<AssignmentFromCourseResponse> Assignments { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve details of Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<OfferingDetailsResponse> query = await _sender.Send(new GetOfferingDetailsQuery(Id));
        
        if (query.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), query.Error, true)
                .Warning("Failed to retrieve details of Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                query.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" }));

            return;
        }

        Offering = query.Value;

        Result<List<AssignmentFromCourseResponse>> assignments = await _sender.Send(new GetRubricAssignmentsFromCourseQuery(Offering.Id));

        if (assignments.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), query.Error, true)
                .Warning("Failed to retrieve details of Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

            //ModalContent = ErrorDisplay.Create(assignments.Error);

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
        UnenrolStudentModalViewModel viewModel = new(
            Id,
            studentId,
            studentName,
            courseName,
            offeringName);

        return Partial("UnenrolStudentModal", viewModel);
    }

    public async Task<IActionResult> OnGetUnenrolStudent(
        StudentId studentId)
    {
        UnenrolStudentFromOfferingCommand fromOfferingCommand = new(studentId, Id);

        _logger
            .ForContext(nameof(UnenrolStudentFromOfferingCommand), fromOfferingCommand, true)
            .Information("Requested to remove Student from Offering by user {User}", _currentUserService.UserName);

        Result request = await _sender.Send(fromOfferingCommand);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to remove Student from Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxRemoveSession(
        string sessionPeriod,
        SessionId sessionId,
        string courseName,
        string offeringName)
    {
        RemoveSessionModalViewModel viewModel = new(
            Id,
            sessionId,
            sessionPeriod,
            courseName,
            offeringName);

        return Partial("RemoveSessionModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveSession(SessionId sessionId)
    {
        RemoveSessionCommand command = new(Id, sessionId);

        _logger
            .ForContext(nameof(RemoveSessionCommand), command, true)
            .Information("Requested to remove Session from Offering by user {User}", _currentUserService.UserName);

        Result request = await _sender.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to remove Session from Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
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
        RemoveAllSessionsModalViewModel viewModel = new(
            Id,
            courseName,
            offeringName);

        return Partial("RemoveAllSessionsModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveAllSessions()
    {
        RemoveAllSessionsCommand command = new(Id);

        _logger
            .ForContext(nameof(RemoveAllSessionsCommand), command, true)
            .Information("Requested to remove all Sessions from Offering by user {User}", _currentUserService.UserName);

        Result request = await _sender.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to remove all Sessions from Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
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
        if (viewModel.AssignmentType is null)
        {
            _logger
                .ForContext(nameof(Error), TeacherAssignmentErrors.InvalidType, true)
                .Warning("Failed to add Teacher to Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(TeacherAssignmentErrors.InvalidType);

            await PreparePage();

            return Page();
        }

        AddTeacherToOfferingCommand command = new(Id, viewModel.StaffId, viewModel.AssignmentType);

        _logger
            .ForContext(nameof(AddTeacherToOfferingCommand), command, true)
            .Information("Requested to add Teacher to Offering by user {User}", _currentUserService.UserName);

        Result request = await _sender.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add Teacher to Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEnrolStudent(
        EnrolStudentInOfferingSelection viewModel)
    {
        EnrolStudentInOfferingCommand inOfferingCommand = new(viewModel.StudentId, Id);

        _logger
            .ForContext(nameof(EnrolStudentInOfferingCommand), inOfferingCommand, true)
            .Information("Requested to add Student to Offering by user {User}", _currentUserService.UserName);

        Result request = await _sender.Send(inOfferingCommand);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add Student to Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

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
        RemoveTeacherFromOfferingModalViewModel viewModel = new(
            Id,
            staffId,
            teacherName,
            assignmentType,
            courseName,
            offeringName);

        return Partial("RemoveTeacherFromOfferingModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveTeacher(
        StaffId staffId,
        [ModelBinder(typeof(FromValueBinder))] AssignmentType assignmentType)
    {
        RemoveTeacherFromOfferingCommand command = new(Id, staffId, assignmentType);

        _logger
            .ForContext(nameof(RemoveTeacherFromOfferingCommand), command, true)
            .Information("Requested to remove Teacher from Offering by user {User}", _currentUserService.UserName);

        Result request = await _sender.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to remove Teacher from Offering by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(
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
        ResourceId resourceId)
    {
    RemoveResourceFromOfferingModalViewModel viewModel = new(
            Id,
            resourceId,
            resourceName,
            resourceType,
            courseName,
            offeringName);

        return Partial("RemoveResourceFromOfferingModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveResource(ResourceId resourceId)
    {
        RemoveResourceFromOfferingCommand command = new(Id, resourceId);

        _logger
            .ForContext(nameof(RemoveResourceFromOfferingCommand), command, true)
            .Information("Requested to remove Resource from Offering by user {User}", _currentUserService.UserName);

        Result request = await _sender.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to remove Resource from Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
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
        if (viewModel.PeriodId == PeriodId.Empty)
        {
            ModalContent = ErrorDisplay.Create(PeriodErrors.InvalidId);

            await PreparePage();

            return Page();
        }

        AddSessionToOfferingCommand command = new(Id, viewModel.PeriodId);
        
        _logger
            .ForContext(nameof(AddSessionToOfferingCommand), command, true)
            .Information("Requested to add Session to Offering by user {User}", _currentUserService.UserName);
        
        Result request = await _sender.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add Session to Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            await PreparePage();
            
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadRubricResults(
        [ModelBinder(typeof(FromValueBinder))] CanvasCourseCode courseCode,
        int assignmentId,
        string assignmentName)
    {
        ExportCanvasRubricResultsQuery command = new(Id, courseCode, assignmentId, assignmentName);

        _logger
            .ForContext(nameof(ExportCanvasRubricResultsQuery), command, true)
            .Information("Requested to export Canvas Assignment Rubric Results for Offering by User {User}", _currentUserService.UserName);

        Result<FileDto> request = await _sender.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to export Canvas Assignment Rubric Results for Offering by User {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            await PreparePage();

            return Page();
        }

        return File(request.Value.FileData, request.Value.FileType, request.Value.FileName);
    }

    public async Task<IActionResult> OnGetDownloadComments(
        [ModelBinder(typeof(FromValueBinder))] CanvasCourseCode courseCode,
        int assignmentId,
        string assignmentName)
    {
        ExportCanvasAssignmentCommentsQuery command = new(Id, courseCode, assignmentId, assignmentName);

        _logger
            .ForContext(nameof(ExportCanvasAssignmentCommentsQuery), command, true)
            .Information("Requested to export Canvas Assignment Comments for Offering by User {User}", _currentUserService.UserName);

        Result<FileDto> request = await _sender.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to export Canvas Assignment Comments for Offering by User {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            await PreparePage();

            return Page();
        }

        return File(request.Value.FileData, request.Value.FileType, request.Value.FileName);
    }
}
