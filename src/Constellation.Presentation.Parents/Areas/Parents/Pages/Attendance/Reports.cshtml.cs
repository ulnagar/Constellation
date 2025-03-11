namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Attendance;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Models.Auth;
using Application.Students.GetStudentsByParentEmail;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.Attendance.GetValidAttendanceReportDates;
using Constellation.Application.Models.Identity;
using Constellation.Application.Parents.GetParentWithStudentIds;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
public class ReportsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ReportsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        UserManager<AppUser> userManager,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ReportsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.ParentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Attendance;

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public StudentResponse? SelectedStudent { get; set; }

    public List<StudentResponse> Students { get; set; } = new();

    public List<ValidAttendenceReportDate> ValidDates { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownload(
        DateOnly startDate,
        DateOnly endDate)
    {
        _logger.Information("Requested to retrieve attendance report by parent {name}", _currentUserService.UserName);
        
        bool authorised = await HasAuthorizedAccessToStudent();

        if (!authorised)
        {
            _logger
                .Warning("Unauthorised attempt to download attendance report by parent {name}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            await PreparePage();

            return Page();
        }

        // Create file as stream
        GenerateAttendanceReportForStudentQuery attendanceReportRequest = new(
            StudentId,
            startDate,
            endDate);

        Result<FileDto> fileRequest = await _mediator.Send(attendanceReportRequest);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(GenerateAttendanceReportForStudentQuery), attendanceReportRequest, true)
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed attempt to download attendance report by parent {name}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(fileRequest.Error);

            await PreparePage();

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }


    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user}", _currentUserService.UserName);

        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetStudentsByParentEmailQuery(_currentUserService.EmailAddress));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return;
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        if (Students.Count == 1)
            StudentId = Students.First().StudentId;

        if (StudentId != StudentId.Empty)
        {
            _logger.Information("Requested to retrieve absence report dates by parent {name}", _currentUserService.UserName);

            Result<List<ValidAttendenceReportDate>> datesRequest = await _mediator.Send(new GetValidAttendenceReportDatesQuery());

            if (datesRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(datesRequest.Error);

                return;
            }

            ValidDates = datesRequest.Value;
            SelectedStudent = Students.FirstOrDefault(entry => entry.StudentId == StudentId);
        }
    }

    private async Task<bool> HasAuthorizedAccessToStudent()
    {
        AppUser user = await _userManager.FindByEmailAsync(_currentUserService.EmailAddress);

        if (user is null)
            return false;

        Result<List<StudentId>> studentIdRequest = await _mediator.Send(new GetParentWithStudentIdsQuery(user.Email));

        if (studentIdRequest.IsFailure)
            return false;

        if (studentIdRequest.Value.Contains(StudentId))
            return true;

        return false;
    }
}