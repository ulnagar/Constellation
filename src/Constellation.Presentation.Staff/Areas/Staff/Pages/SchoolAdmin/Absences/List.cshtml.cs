namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Constellation.Application.Absences.ExportAbsencesReport;
using Constellation.Application.Absences.GetAbsencesWithFilterForReport;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.Schools.Models;
using Constellation.Application.StaffMembers.GetStaffLinkedToOffering;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ListModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public ListModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Absences_List;

    [BindProperty]
    public FilterDefinition Filter { get; set; } = new();

    public List<ClassRecord> ClassSelectionList { get; set; } = new();

    public List<SchoolSelectionListResponse> SchoolsList { get; set; } = new();

    public Dictionary<string, string> StudentsList { get; set; } = new();

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

        var file = await _mediator.Send(new ExportAbsencesReportCommand(
                offeringIds,
                Filter.Grades,
                Filter.Schools,
                Filter.Students),
            cancellationToken);

        if (file.IsFailure)
        {
            ModalContent = new ErrorDisplay(file.Error);

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        var classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(classesResponse.Error);

            return Page();
        }

        foreach (var course in classesResponse.Value)
        {
            var teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(course.Id), cancellationToken);

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

            var primaryTeacher = teachers.Value.First(teacher => teacher.StaffId == frequency.StaffId);

            ClassSelectionList.Add(new ClassRecord(
                course.Id,
                course.Name,
                $"{primaryTeacher.FirstName[..1]} {primaryTeacher.LastName}",
                $"Year {course.Name[..2]}"));
        }

        var schoolsRequest = await _mediator.Send(new GetCurrentPartnerSchoolsWithStudentsListQuery(), cancellationToken);

        if (schoolsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(schoolsRequest.Error);

            return Page();
        }

        SchoolsList = schoolsRequest.Value;

        var studentsRequest = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return Page();
        }

        StudentsList = studentsRequest.Value;

        List<OfferingId> offeringIds = Filter.Offerings.Select(id => OfferingId.FromValue(id)).ToList();

        var absenceRequest = await _mediator.Send(
            new GetAbsencesWithFilterForReportQuery(
                    offeringIds,
                    Filter.Grades,
                    Filter.Schools,
                    Filter.Students),
                cancellationToken);

        if (absenceRequest.IsFailure)
        {
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
        public List<string> Students { get; set; } = new();

        public FilterAction Action { get; set; } = FilterAction.Filter;

        public enum FilterAction
        {
            Filter,
            Export
        }
    }
}
