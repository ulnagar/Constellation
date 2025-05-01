namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.Absences;

using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetCurrentPartnerSchoolsWithStudentsList;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffLinkedToOffering;
using Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Attendance.Absences.Queries.ExportAbsencesReport;
using Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesWithFilterForReport;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingsForSelectionList;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Constellation.Presentation.Staff.Areas.Staff.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Absences;
    [ViewData] public string PageTitle => "Absences";

    [BindProperty]
    public FilterDefinition Filter { get; set; } = new();

    public List<ClassRecord> ClassSelectionList { get; set; } = new();

    public List<SchoolSelectionListResponse> SchoolsList { get; set; } = new();

    public Dictionary<StudentId, string> StudentsList { get; set; } = new();

    public List<FilteredAbsenceResponse> Absences { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostFilter(CancellationToken cancellationToken)
    {
        if (Filter.Action == FilterDefinition.FilterAction.Filter)
            return await PreparePage(cancellationToken);

        if (Filter.Action == FilterDefinition.FilterAction.Export)
            return await OnPostExport(cancellationToken);

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostExport(CancellationToken cancellationToken)
    {
        List<OfferingId> offeringIds = Filter.Offerings.Select(id => OfferingId.FromValue(id)).ToList();

        ExportAbsencesReportCommand command = new(
            offeringIds,
            Filter.Grades,
            Filter.Schools,
            Filter.Students);

        _logger
            .ForContext(nameof(ExportAbsencesReportCommand), command, true)
            .Information("Requested to export list of Absences by user {User}", _currentUserService.UserName);

        Result<FileDto> file = await _mediator.Send(command, cancellationToken);
        
        if (file.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), file.Error, true)
                .Information("Failed to export list of Absences by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(file.Error);

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve list of Absences by user {User}", _currentUserService.UserName);

        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), classesResponse.Error, true)
                .Warning("Failed to retrieve list of Absences by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(classesResponse.Error);

            return Page();
        }

        foreach (OfferingSelectionListResponse course in classesResponse.Value)
        {
            Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(course.Id), cancellationToken);

            if (teachers.IsFailure || teachers.Value.Count == 0)
            {
                ClassSelectionList.Add(new ClassRecord(
                    course.Id,
                    course.Name,
                    string.Empty,
                    $"Year {course.Name[..2]}"));

                continue;
            }

            var frequency = teachers
                .Value
                .GroupBy(x => x.StaffId)
                .Select(group => new { StaffId = group.Key, Count = group.Count() })
                .OrderByDescending(x => x.Count)
                .First();

            StaffSelectionListResponse primaryTeacher = teachers.Value.First(teacher => teacher.StaffId == frequency.StaffId);

            ClassSelectionList.Add(new ClassRecord(
                course.Id,
                course.Name,
                $"{primaryTeacher.FirstName[..1]} {primaryTeacher.LastName}",
                $"Year {course.Name[..2]}"));
        }

        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetCurrentPartnerSchoolsWithStudentsListQuery(), cancellationToken);

        if (schoolsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), schoolsRequest.Error, true)
                .Warning("Failed to retrieve list of Absences by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(schoolsRequest.Error);

            return Page();
        }

        SchoolsList = schoolsRequest.Value;

        Result<Dictionary<StudentId, string>> studentsRequest = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

        if (studentsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentsRequest.Error, true)
                .Warning("Failed to retrieve list of Absences by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return Page();
        }

        StudentsList = studentsRequest.Value;

        List<OfferingId> offeringIds = Filter.Offerings.Select(OfferingId.FromValue).ToList();

        Result<List<FilteredAbsenceResponse>> absenceRequest = await _mediator.Send(
            new GetAbsencesWithFilterForReportQuery(
                    offeringIds,
                    Filter.Grades,
                    Filter.Schools,
                    Filter.Students),
                cancellationToken);

        if (absenceRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), absenceRequest.Error, true)
                .Warning("Failed to retrieve list of Absences by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(absenceRequest.Error);

            return Page();
        }

        Absences = absenceRequest
            .Value
            .OrderBy(absence => absence.Grade)
            .ThenBy(absence => absence.Student.LastName)
            .ThenBy(absence => absence.Student.FirstName)
            .ThenByDescending(absence => absence.AbsenceDate)
            .ToList();

        return Page();
    }

    public class FilterDefinition
    {
        public List<Guid> Offerings { get; set; } = new();
        public List<Grade> Grades { get; set; } = new();
        public List<string> Schools { get; set; } = new();
        public List<StudentId> Students { get; set; } = new();

        public FilterAction Action { get; set; } = FilterAction.Filter;

        public enum FilterAction
        {
            Filter,
            Export
        }
    }
}
