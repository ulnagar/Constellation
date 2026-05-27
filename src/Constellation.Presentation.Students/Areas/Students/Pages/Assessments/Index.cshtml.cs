namespace Constellation.Presentation.Students.Areas.Students.Pages.Assessments;

using Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsByStudentId;
using Application.Domains.Students.Models;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Assessments.Assessments.Models;
using Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownloadFile;
using Constellation.Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using Constellation.Application.Domains.Students.Queries.GetStudentById;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Assessments.Identifiers;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.StudentPortal_View_Value)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }

    public Dictionary<SchoolCalendarWeek, List<AssessmentDetailsResponse>> Assessments { get; set; } = new();

    public StudentResponse Student { get; set; }

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        _logger.Information("Requested to load assessments by user {user}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Information("Requested to load assessments by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(studentId));

        if (studentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Information("Requested to load assessments by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(studentRequest.Error);

            return;
        }

        Student = studentRequest.Value;

        GetCurrentAssessmentsByStudentIdQuery query = new(studentId);

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
