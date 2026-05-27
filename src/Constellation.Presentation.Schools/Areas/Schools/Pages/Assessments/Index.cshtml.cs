namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Assessments;

using Application.Domains.Assessments.Assessments.Models;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownloadFile;
using Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsBySchoolCode;
using Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Attachments.Queries.GetAttachmentFile;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Attachments.Enums;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Assessments.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_Assessments_View_Value)]
public class IndexModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Assessments;

    public Dictionary<SchoolCalendarWeek, List<AssessmentDetailsResponse>> Assessments { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        GetCurrentAssessmentsBySchoolCodeQuery query = new(CurrentSchoolCode);

        _logger
            .ForContext(nameof(GetCurrentAssessmentsBySchoolCodeQuery), query, true)
            .Information("Requested to load assessments by user {user}", _currentUserService.UserName);

        Result<List<AssessmentDetailsResponse>> assessments = await _mediator.Send(query);

        if (assessments.IsFailure)
        {
            _logger
                .ForContext(nameof(GetCurrentAssessmentsBySchoolCodeQuery), query, true)
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