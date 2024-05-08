namespace Constellation.Presentation.Server.Areas.Partner.Pages.Contacts;

using Application.DTOs;
using Application.StaffMembers.Models;
using Constellation.Application.Contacts.ExportContactList;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.Contacts.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.Schools.Models;
using Constellation.Application.StaffMembers.GetStaffLinkedToOffering;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Shared.Models;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty]
    public FilterDefinition Filter { get; set; } = new();

    public List<ContactResponse> Contacts { get; set; } = new();

    public List<ClassRecord> ClassSelectionList { get; set; } = new();

    public List<SchoolSelectionListResponse> SchoolsList { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);

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
        List<ContactCategory> filterCategories = new();

        foreach (string entry in Filter.Categories)
            filterCategories.Add(ContactCategory.FromValue(entry));

        List<OfferingId> offeringIds = Filter.Offerings.Select(OfferingId.FromValue).ToList();

        Result<FileDto> file = await _mediator.Send(new ExportContactListCommand(
            offeringIds,
            Filter.Grades,
            Filter.Schools,
            filterCategories),
            cancellationToken);

        if (file.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = file.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = classesResponse.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        foreach (OfferingSelectionListResponse course in classesResponse.Value)
        {
            Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(course.Id), cancellationToken);

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
                RedirectPath = _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        SchoolsList = schoolsRequest.Value;

        List<ContactCategory> filterCategories = new();

        foreach (string entry in Filter.Categories)
            filterCategories.Add(ContactCategory.FromValue(entry));

        List<OfferingId> offeringIds = Filter.Offerings.Select(OfferingId.FromValue).ToList();

        if (offeringIds.Any() ||
            filterCategories.Any() ||
            Filter.Grades.Any() ||
            Filter.Schools.Any())
        {
            Result<List<ContactResponse>> contactRequest = await _mediator.Send(
                new GetContactListQuery(
                    offeringIds,
                    Filter.Grades,
                    Filter.Schools,
                    filterCategories),
                cancellationToken);

            if (contactRequest.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = contactRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" })
                };

                return Page();
            }

            Contacts = contactRequest.Value;

            Contacts = Contacts
                .OrderBy(contact => contact.StudentGrade)
                .ThenBy(contact => contact.Student.LastName)
                .ThenBy(contact => contact.Student.FirstName)
                .ToList();
        }

        return Page();
    }

    public class FilterDefinition
    {
        public List<Guid> Offerings { get; set; } = new();
        public List<Grade> Grades { get; set; } = new();
        public List<string> Schools { get; set; } = new();
        public List<string> Categories { get; set; } = new();

        public FilterAction Action { get; set; } = FilterAction.Filter;

        public enum FilterAction
        {
            Filter,
            Export,
            Email
        }
    }
}
