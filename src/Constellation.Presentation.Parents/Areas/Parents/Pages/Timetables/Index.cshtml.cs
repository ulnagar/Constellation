namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Timetables;

using Application.Models.Auth;
using Application.Students.GetStudentsByParentEmail;
using Application.Timetables.GetStudentTimetableExport;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Timetables.GetStudentTimetableData;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using Core.Models.Timetables.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.ParentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Timetables;

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public StudentResponse? SelectedStudent { get; set; }

    public List<StudentResponse> Students { get; set; } = new();

    public StudentTimetableDataDto TimetableData { get; set; }
    public IEnumerable<PeriodWeek> Weeks { get; set; }

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(IntEnumBinder))]
    public PeriodDay? Day { get; set; }

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(IntEnumBinder))]
    public PeriodWeek? Week { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownload()
    {
        _logger.Information("Requested to export timetable by user {user} for student {student}", _currentUserService.UserName, StudentId);

        Result<FileDto> fileResponse = await _mediator.Send(new GetStudentTimetableExportQuery(StudentId));

        if (fileResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                fileResponse.Error,
                _linkGenerator.GetPathByPage("/Timetables/Index", values: new { area = "Parents" }));

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
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
            _logger.Information("Requested to retrieve timetable by user {user} for student {student}", _currentUserService.UserName, StudentId);

            Result<StudentTimetableDataDto> timetableRequest = await _mediator.Send(new GetStudentTimetableDataQuery(StudentId));

            if (timetableRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(timetableRequest.Error);

                return;
            }

            TimetableData = timetableRequest.Value;

            Weeks = PeriodWeek.GetOptions;

            if (Day is null && Week is null)
            {
                Day = PeriodDay.Monday;
                Week = Weeks.First();
            }

            SelectedStudent = Students.FirstOrDefault(entry => entry.StudentId == StudentId);
        }
    }
}