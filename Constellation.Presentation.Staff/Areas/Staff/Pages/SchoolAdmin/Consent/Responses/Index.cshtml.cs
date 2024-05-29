namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Responses;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Models.Auth;
using Application.Schools.Models;
using Application.StaffMembers.Models;
using Application.ThirdPartyConsent.GetTransactionsWithFilter;
using Constellation.Application.Absences.ExportAbsencesReport;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.StaffMembers.GetStaffLinkedToOffering;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Application.ThirdPartyConsent.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Transactions;

    [BindProperty]
    public FilterDefinition Filter { get; set; } = new();

    public List<ClassRecord> ClassSelectionList { get; set; } = new();

    public List<SchoolSelectionListResponse> SchoolsList { get; set; } = new();

    public Dictionary<string, string> StudentsList { get; set; } = new();

    public List<TransactionSummaryResponse> Transactions { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);

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
        List<OfferingId> offeringIds = Filter.Offerings.Select(OfferingId.FromValue).ToList();

        //TODO: Create export option here if required

        return await PreparePage(cancellationToken);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = classesResponse.Error,
                RedirectPath = null
            };

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
            Error = new ErrorDisplay
            {
                Error = schoolsRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        SchoolsList = schoolsRequest.Value;

        Result<Dictionary<string, string>> studentsRequest = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

        if (studentsRequest.IsFailure)
        {
            Error = new()
            {
                Error = studentsRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        StudentsList = studentsRequest.Value;

        List<OfferingId> offeringIds = Filter.Offerings.Select(id => OfferingId.FromValue(id)).ToList();

        Result<List<TransactionSummaryResponse>> result = await _mediator.Send(new GetTransactionsWithFilterQuery(
            offeringIds,
            Filter.Grades,
            Filter.Schools,
            Filter.Students), cancellationToken);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            return Page();
        }

        Transactions = result.Value
            .OrderBy(entry => entry.Grade)
            .ThenBy(entry => entry.Name.SortOrder)
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