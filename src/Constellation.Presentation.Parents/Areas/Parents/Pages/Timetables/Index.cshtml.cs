namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Timetables;

using Application.Models.Auth;
using Application.Students.GetStudentsByParentEmail;
using Application.Timetables.GetStudentTimetableExport;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Reports.GetAcademicReportList;
using Constellation.Application.Timetables.GetStudentTimetableData;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
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
            .ForContext("APPLICATION", "Parent Portal");
    }

    [ViewData] public string ActivePage => Models.ActivePage.Timetables;

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; } = string.Empty;

    public StudentResponse? SelectedStudent { get; set; }

    public List<StudentResponse> Students { get; set; } = new();

    public StudentTimetableDataDto TimetableData { get; set; }
    public Dictionary<int, string> Weeks { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public SchoolDay? Day { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Week { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownload()
    {
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

        if (!string.IsNullOrWhiteSpace(StudentId))
        {
            _logger.Information("Requested to retrieve reports by user {user} for student {student}", _currentUserService.UserName, StudentId);

            Result<StudentTimetableDataDto> timetableRequest = await _mediator.Send(new GetStudentTimetableDataQuery(StudentId));

            if (timetableRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(timetableRequest.Error);

                return;
            }

            TimetableData = timetableRequest.Value;

            List<int> dayList = TimetableData.Timetables.Select(data => data.Day).Distinct().ToList();
            int weekList = dayList.Max();
            int numWeeks = weekList / 5;

            Weeks = new();

            foreach (int entry in Enumerable.Range(1, numWeeks))
            {
                switch (entry)
                {
                    case 1:
                        Weeks.Add(1, "Week A");
                        break;
                    case 2:
                        Weeks.Add(2, "Week B");
                        break;
                    case 3:
                        Weeks.Add(3, "Week C");
                        break;
                    case 4:
                        Weeks.Add(4, "Week D");
                        break;
                    default:
                        break;
                }
            }

            if (Day is null && Week is null)
            {
                Day = SchoolDay.Monday;
                Week = Weeks.First().Key;
            }

            SelectedStudent = Students.FirstOrDefault(entry => entry.StudentId == StudentId);
        }
    }

    public enum SchoolDay
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5
    }
}