namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Assessments;

using Application.Domains.Assessments.Assessments.Models;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownloadFile;
using Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsBySchoolCode;
using Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Assessments.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public IndexModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Assessments;

    public Dictionary<SchoolCalendarWeek, List<AssessmentDetailsResponse>> Assessments { get; set; } = new();
    public bool CanAccessRestrictedFiles { get; set; }
    public bool CanSubmitFiles { get; set; }

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

        TimeZoneInfo aest = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");

        List<AssessmentDetailsResponse> unmatchedAssessments = new();

        List<SchoolCalendarWeek> orderedWeeks = calendarWeeks.Value.OrderBy(w => w.StartDate).ToList();

        Dictionary<string, DateTime> termEndDates = orderedWeeks
            .GroupBy(w => w.TermGroup)
            .ToDictionary(
                g => g.Key,
                g => g.Max(w => w.EndDate));

        // Pre-compute effective end bounds for each week
        var weekBounds = orderedWeeks.Select((week, i) =>
        {
            DateTime termEnd = termEndDates[week.TermGroup];

            // Extend to Sunday, but cap at either:
            // - the day before the next week starts, or
            // - the term's actual end date
            // whichever is earlier
            int daysToSunday = (7 - (int)week.EndDate.DayOfWeek) % 7;
            
            DateTime sunday = week.EndDate.AddDays(daysToSunday);

            DateTime nextWeekStart = i < orderedWeeks.Count - 1
                ? orderedWeeks[i + 1].StartDate.AddDays(-1)
                : termEnd;

            DateTime effectiveEnd = new[] { sunday, nextWeekStart, termEnd }
                .Min();

            // Extend to end-of-day so any time on the final day is included
            DateTime effectiveEndEod = effectiveEnd.Date.AddDays(1).AddTicks(-1);
            
            return new
            {
                Week = week,
                StartBound = new DateTimeOffset(week.StartDate, aest.GetUtcOffset(week.StartDate)),
                EndBound = new DateTimeOffset(effectiveEndEod, aest.GetUtcOffset(effectiveEndEod))
            };
        }).ToList();

        foreach (AssessmentDetailsResponse assessment in assessments.Value)
        {
            var matchedWeek = weekBounds.FirstOrDefault(wb =>
                assessment.DueDate >= wb.StartBound &&
                assessment.DueDate <= wb.EndBound);

            if (matchedWeek is not null)
            {
                if(Assessments.TryGetValue(matchedWeek.Week, out var entry))
                    entry.Add(assessment);
                else
                    Assessments.Add(matchedWeek.Week, [ assessment ]);
            }
            else
                unmatchedAssessments.Add(assessment);
        }

        foreach (AssessmentDetailsResponse assessment in unmatchedAssessments)
        {
            var priorWeek = weekBounds
                .Where(entry => entry.EndBound < assessment.DueDate)
                .MaxBy(entry => entry.EndBound);

            if (priorWeek is null)
            {
                // Must be at the start of the year.
                SchoolCalendarWeek matchedWeek = new(string.Empty,
                    _dateTime.FirstDayOfYear.ToDateTime(TimeOnly.MinValue),
                    _dateTime.FirstDayOfYear.ToDateTime(TimeOnly.MinValue), "School Holidays");

                if (Assessments.TryGetValue(matchedWeek, out var entry))
                    entry.Add(assessment);
                else
                    Assessments.Add(matchedWeek, [assessment]);

                continue;
            }

            var nextWeek = weekBounds
                .Where(entry => entry.StartBound > assessment.DueDate)
                .MinBy(entry => entry.StartBound);

            if (nextWeek is null)
            {
                // Must be at the end of the year.
                SchoolCalendarWeek matchedWeek = new(string.Empty,
                    _dateTime.LastDayOfYear.ToDateTime(TimeOnly.MinValue),
                    _dateTime.LastDayOfYear.ToDateTime(TimeOnly.MinValue), "School Holidays");

                if (Assessments.TryGetValue(matchedWeek, out var entry))
                    entry.Add(assessment);
                else
                    Assessments.Add(matchedWeek, [assessment]);
            }
            else
            {
                SchoolCalendarWeek matchedWeek = new(string.Empty,
                    priorWeek.EndBound.Date.AddDays(1),
                    nextWeek.StartBound.Date.AddDays(-1),
                    "School Holidays");

                if (Assessments.TryGetValue(matchedWeek, out var entry))
                    entry.Add(assessment);
                else
                    Assessments.Add(matchedWeek, [assessment]);
            }
        }

        AuthorizationResult canAccessRestrictedFilesTest = await AuthService.AuthorizeAsync(User, AuthPermission.SchoolsPortal_Assessments_ViewRestrictedDocuments_Value);
        if (canAccessRestrictedFilesTest.Succeeded)
            CanAccessRestrictedFiles = true;

        AuthorizationResult canSubmitFilesTest = await AuthService.AuthorizeAsync(User, AuthPermission.SchoolsPortal_Assessments_Submit_Value);
        if (canSubmitFilesTest.Succeeded)
            CanSubmitFiles = true;
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