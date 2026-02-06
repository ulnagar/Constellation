namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.CheckIn;

using Application.Common.PresentationModels;
using Application.Domains.Attendance.CheckIns.Queries.GetCheckInResponses;
using Application.Domains.Attendance.CheckIns.Queries.GetSentimentList;
using Application.Domains.Courses.Models;
using Application.Domains.Courses.Queries.GetCoursesForSelectionList;
using Application.Domains.Courses.Queries.GetCourseSummary;
using Application.Models.Auth;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingsForSelectionList;
using Constellation.Application.Domains.Schools.Models;
using Constellation.Application.Domains.Schools.Queries.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.Domains.StaffMembers.Models;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffLinkedToOffering;
using Constellation.Core.Enums;
using Constellation.Presentation.Staff.Areas.Staff.Models;
using Core.Abstractions.Services;
using Core.Models.Attendance.Checkin;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using System.Threading;

[HasPermission(AuthPermission.StudentAdmin_AttendanceList_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<IndexModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_CheckIn;
    [ViewData] public string PageTitle => "Check In Data";

    [BindProperty]
    public CheckInFilter? Filter { get; set; } = null;

    public List<CheckInResponse> Responses { get; set; } = [];

    public List<ClassRecord> ClassSelectionList { get; set; } = [];
    public List<CourseRecord> CourseSelectedList { get; set; } = [];
    public List<SchoolSelectionListResponse> SchoolsList { get; set; } = [];
    public List<string> SentimentList { get; set; } = [];

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostFilter(CancellationToken cancellationToken = default)
    {
        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostExport(CancellationToken cancellationToken = default)
    {
        return await PreparePage(cancellationToken);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(classesResponse.Error);

            _logger
                .ForContext(nameof(Error), classesResponse.Error, true)
                .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

            return Page();
        }

        foreach (OfferingSelectionListResponse offering in classesResponse.Value)
        {
            Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(offering.Id), cancellationToken);

            if (teachers.Value.Count == 0)
                continue;

            var frequency = teachers
                .Value
                .GroupBy(x => x.StaffId)
                .Select(group => new { StaffId = group.Key, Count = group.Count() })
                .OrderByDescending(x => x.Count)
                .First();

            StaffSelectionListResponse primaryTeacher = teachers.Value.First(teacher => teacher.StaffId == frequency.StaffId);

            ClassSelectionList.Add(new ClassRecord(
                offering.Id,
                offering.Name,
                $"{primaryTeacher.Name.PreferredName[..1]} {primaryTeacher.Name.LastName}",
                $"Year {offering.Name[..2]}"));
        }

        Result<List<CourseSelectListItemResponse>> courses = await _mediator.Send(new GetCoursesForSelectionListQuery(true), cancellationToken);

        if (courses.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(courses.Error);

            _logger
                .ForContext(nameof(Error), courses.Error, true)
                .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

            return Page();
        }

        foreach (var course in courses.Value)
        {
            CourseSelectedList.Add(new CourseRecord(
                course.Id,
                course.Name,
                course.Grade));
        }

        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetCurrentPartnerSchoolsWithStudentsListQuery(), cancellationToken);

        if (schoolsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(schoolsRequest.Error);
            
            _logger
                .ForContext(nameof(Error), schoolsRequest.Error, true)
                .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

            return Page();
        }

        SchoolsList = schoolsRequest.Value;

        Result<List<string>> sentimentResult = await _mediator.Send(new GetSentimentListQuery(), cancellationToken);

        if (sentimentResult.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(sentimentResult.Error);

            _logger
                .ForContext(nameof(Error), sentimentResult.Error, true)
                .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

            return Page();
        }

        SentimentList = sentimentResult.Value;

        Result<List<CheckInResponse>> responses = await _mediator.Send(new GetCheckInResponsesQuery(Filter), cancellationToken);

        if (responses.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(responses.Error);

            return Page();
        }

        Responses = responses.Value;

        return Page();
    }
}