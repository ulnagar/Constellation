namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.GroupTutorials.Tutorials;

using Application.DTOs;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.GroupTutorials.AddStudentToTutorial;
using Constellation.Application.GroupTutorials.AddTeacherToTutorial;
using Constellation.Application.GroupTutorials.CreateRoll;
using Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;
using Constellation.Application.GroupTutorials.RemoveStudentFromTutorial;
using Constellation.Application.GroupTutorials.RemoveTeacherFromTutorial;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using Shared.Components.TutorialRollCreate;
using Shared.Components.TutorialStudentEnrolment;
using Shared.Components.TutorialTeacherAssignment;
using Shared.PartialViews.RemoveStudentFromTutorialModal;
using Shared.PartialViews.RemoveTeacherFromTutorialModal;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanViewGroupTutorials)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_GroupTutorials_Tutorials;
    [ViewData] public string PageTitle { get; set; } = "Tutorial Details";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public GroupTutorialId Id { get; set; } = GroupTutorialId.Empty;
    
    public GroupTutorialDetailResponse Tutorial { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await PreparePage(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostEnrolStudent(
        TutorialStudentEnrolmentSelection viewModel)
    {
        DateOnly? effectiveDate = (viewModel.LimitedTime) ? DateOnly.FromDateTime(viewModel.EffectiveTo) : null;

        AddStudentToTutorialCommand command = new(Id, viewModel.StudentId, effectiveDate);

        _logger
            .ForContext(nameof(AddStudentToTutorialCommand), command, true)
            .Information("Requested to enrol student in Group Tutorial by user {User}", _currentUserService.UserName);

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditGroupTutorials);

        if (!isAuthorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Warning("Failed to enrol student in Group Tutorial by user {User}", _currentUserService.UserName);

            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        if (string.IsNullOrWhiteSpace(viewModel.StudentId))
        {
            _logger
                .ForContext(nameof(Error), ValidationErrors.String.RequiredIsNull(nameof(viewModel.StudentId)), true)
                .Warning("Failed to enrol student in Group Tutorial by user {User}", _currentUserService.UserName);

            await PreparePage();
            return Page();
        }
        
        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to enrol student in Group Tutorial by user {User}", _currentUserService.UserName);

            return ShowError(result.Error);
        }
        
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAssignTeacher(
        TutorialTeacherAssignmentSelection viewModel)
    {
        DateOnly? effectiveDate = (viewModel.LimitedTime) ? DateOnly.FromDateTime(viewModel.EffectiveTo) : null;

        AddTeacherToTutorialCommand command = new(Id, viewModel.StaffId, effectiveDate);

        _logger
            .ForContext(nameof(AddTeacherToTutorialCommand), command, true)
            .Information("Requested to add teacher to Group Tutorial by user {User}", _currentUserService.UserName);

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditGroupTutorials);

        if (!isAuthorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Warning("Failed to add teacher to Group Tutorial by user {User}", _currentUserService.UserName);

            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Warning("Failed to add teacher to Group Tutorial by user {User}", _currentUserService.UserName);

            await PreparePage();
            return Page();
        }
        
        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Warning("Failed to add teacher to Group Tutorial by user {User}", _currentUserService.UserName);

            return ShowError(result.Error);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxRemoveTeacher(
        [ModelBinder(typeof(ConstructorBinder))] TutorialTeacherId teacherId)
    {
        Result<GroupTutorialDetailResponse> result = await _mediator.Send(new GetTutorialWithDetailsByIdQuery(Id));

        if (result.IsFailure)
            return Content(string.Empty);

        TutorialTeacherResponse? teacherRecord = result.Value.Teachers.FirstOrDefault(teacher => teacher.Id == teacherId);

        if (teacherRecord is null)
            return Content(string.Empty);

        RemoveTeacherFromTutorialModalViewModel viewModel = new()
        {
            Id = teacherRecord.Id, 
            Name = teacherRecord.Name
        };

        return Partial("RemoveTeacherFromTutorialModal", viewModel);
    }

    public async Task<IActionResult> OnPostRemoveTeacher(
        RemoveTeacherFromTutorialModalViewModel viewModel)
    {
        DateOnly? effectiveDate = (!viewModel.Immediate) ? DateOnly.FromDateTime(viewModel.EffectiveOn) : null;

        RemoveTeacherFromTutorialCommand command = new(Id, viewModel.Id, effectiveDate);

        _logger
            .ForContext(nameof(RemoveTeacherFromTutorialCommand), command, true)
            .Information("Requested to remove teacher from Group Tutorial by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove teacher from Group Tutorial by user {User}", _currentUserService.UserName);

            return ShowError(result.Error);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxRemoveStudent(
        [ModelBinder(typeof(ConstructorBinder))] TutorialEnrolmentId enrolmentId)
    {
        Result<GroupTutorialDetailResponse> result = await _mediator.Send(new GetTutorialWithDetailsByIdQuery(Id));

        if (result.IsFailure)
            return Content(string.Empty);

        TutorialEnrolmentResponse? enrolmentRecord = result.Value.Students.FirstOrDefault(enrolment => enrolment.Id == enrolmentId);

        if (enrolmentRecord is null)
            return Content(string.Empty);

        RemoveStudentFromTutorialModalViewModel viewModel = new()
        {
            Id = enrolmentId,
            Name = enrolmentRecord.Name
        };

        return Partial("RemoveStudentFromTutorialModal", viewModel);
    }

    public async Task<IActionResult> OnPostRemoveStudent(
        RemoveStudentFromTutorialModalViewModel viewModel)
    {
        DateOnly? effectiveDate = (!viewModel.Immediate) ? DateOnly.FromDateTime(viewModel.EffectiveOn) : null;

        RemoveStudentFromTutorialCommand command = new(Id, viewModel.Id, effectiveDate);

        _logger
            .ForContext(nameof(RemoveStudentFromTutorialCommand), command, true)
            .Information("Requested to remove student from Group Tutorial by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove student from Group Tutorial by user {User}", _currentUserService.UserName);

            return ShowError(result.Error);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateRoll(
        TutorialRollCreateSelection viewModel)
    {
        CreateRollCommand command = new(Id, DateOnly.FromDateTime(viewModel.RollDate));

        _logger
            .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
            .Warning("Failed to create roll for Group Tutorial by user {User}", _currentUserService.UserName);

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, Id, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Warning("Failed to create roll for Group Tutorial by user {User}", _currentUserService.UserName);

            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        Result<TutorialRollId> result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create roll for Group Tutorial by user {User}", _currentUserService.UserName);

            return ShowError(result.Error);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        _logger.Information("Requested to download Attendance Report for Group Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<FileDto> fileDto = await _mediator.Send(new GenerateTutorialAttendanceReportQuery(Id));

        if (fileDto.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileDto.Error, true)
                .Warning("Failed to download Attendance Report for Group Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

            ShowError(fileDto.Error);
        }

        return File(fileDto.Value.FileData, fileDto.Value.FileType, fileDto.Value.FileName);
    }

    private async Task PreparePage(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve details of GroupTutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<GroupTutorialDetailResponse> result = await _mediator.Send(new GetTutorialWithDetailsByIdQuery(Id), cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to retrieve details of GroupTutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Index", values: new { area = "Staff" }));

            Tutorial = new(
                Id,
                null,
                DateOnly.MinValue,
                DateOnly.MinValue,
                new List<TutorialTeacherResponse>(),
                new List<TutorialEnrolmentResponse>(),
                new List<TutorialRollResponse>());
        }
        else
        {
            Tutorial = result.Value;
            PageTitle = $"Details - {Tutorial.Name}";
        }
    }

    private IActionResult ShowError(Error error)
    {
        ModalContent = new ErrorDisplay(
            error,
            _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Details", values: new { area = "Staff", Id = Id }));

        Tutorial = new(
            Id,
            null,
            DateOnly.MinValue,
            DateOnly.MinValue,
            new List<TutorialTeacherResponse>(),
            new List<TutorialEnrolmentResponse>(),
            new List<TutorialRollResponse>());
        
        return Page();
    }
}
