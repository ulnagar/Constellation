namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Assessments;

using Application.Common.PresentationModels;
using Application.Domains.Assessments.Assessments.Models;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownloadFile;
using Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsByStudentId;
using Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using Application.Domains.Students.Queries.GetStudentsByParentEmail;
using Application.Models.Auth;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Assessments.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.ParentPortal_View_Value)]
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

    [ViewData] public string ActivePage => Models.ActivePage.Assessments;

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public StudentResponse? SelectedStudent { get; set; }

    public List<StudentResponse> Students { get; set; } = new();

    public Dictionary<SchoolCalendarWeek, List<AssessmentDetailsResponse>> Assessments { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user}", _currentUserService.UserName);

        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetStudentsByParentEmailQuery(_currentUserService.EmailAddress));

        if (studentsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(studentsRequest.Error);

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
            SelectedStudent = Students.FirstOrDefault(entry => entry.StudentId == StudentId);

            GetCurrentAssessmentsByStudentIdQuery query = new(StudentId);

            _logger
                .ForContext(nameof(GetCurrentAssessmentsByStudentIdQuery), query, true)
                .Information("Requested to load assessments by user {user}", _currentUserService.UserName);

            Result<List<AssessmentDetailsResponse>> assessments = await _mediator.Send(query);

            if (assessments.IsFailure)
            {
                _logger
                    .ForContext(nameof(GetCurrentAssessmentsByStudentIdQuery), query, true)
                    .ForContext(nameof(Error), assessments.Error, true)
                    .Warning("Failed to load assessments by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(assessments.Error);

                return;
            }

            Result<List<SchoolCalendarWeek>> calendarWeeks = await _mediator.Send(new GetTermsAndWeeksForCurrentYearQuery());

            if (calendarWeeks.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), calendarWeeks.Error, true)
                    .Warning("Failed to load assessments by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(calendarWeeks.Error);

                return;
            }

            foreach (var week in calendarWeeks.Value.OrderBy(entry => entry.StartDate))
            {
                List<AssessmentDetailsResponse> matchingAssessments = assessments.Value
                    .Where(entry =>
                        entry.DueDate >= week.StartDate
                        && entry.DueDate <= week.EndDate)
                    .ToList();

                if (matchingAssessments.Count > 0)
                {
                    Assessments.Add(week, matchingAssessments);
                }
            }
        }
    }

    public async Task<IActionResult> OnGetDownloadFile(AssessmentId assessmentId, AssessmentDownloadId downloadId)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAssessmentDownloadFileQuery(assessmentId, downloadId));

        if (fileResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(fileResponse.Error);

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }
}