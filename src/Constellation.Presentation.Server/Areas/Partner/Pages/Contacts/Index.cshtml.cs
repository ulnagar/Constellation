namespace Constellation.Presentation.Server.Areas.Partner.Pages.Contacts;

using Constellation.Application.Contacts.ExportContactList;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.Schools.Models;
using Constellation.Application.StaffMembers.GetStaffLinkedToOffering;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Constellation.Presentation.Server.Areas.ShortTerm.Pages.Covers.CreateModel;

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

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        return await PreparePage(cancellationToken);
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
        List<ContactCategory> filterCategories = new();

        foreach (var entry in Filter.Categories)
            filterCategories.Add(ContactCategory.FromValue(entry));

        var file = await _mediator.Send(new ExportContactListCommand(
                Filter.Offerings,
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
        await GetClasses(_mediator);

        var classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = classesResponse.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        foreach (var course in classesResponse.Value)
        {
            var teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(course.Id), cancellationToken);

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
            Error = new ErrorDisplay
            {
                Error = schoolsRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        SchoolsList = schoolsRequest.Value;

        List<ContactCategory> filterCategories = new();

        foreach (var entry in Filter.Categories)
            filterCategories.Add(ContactCategory.FromValue(entry));

        var contactRequest = await _mediator.Send(
            new GetContactListQuery(
                Filter.Offerings,
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

        return Page();
    }

    public class FilterDefinition
    {
        public List<int> Offerings { get; set; } = new();
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