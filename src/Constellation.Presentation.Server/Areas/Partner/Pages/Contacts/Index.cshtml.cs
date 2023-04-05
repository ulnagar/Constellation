namespace Constellation.Presentation.Server.Areas.Partner.Pages.Contacts;

using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.Features.Awards.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
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

    [BindProperty(SupportsGet = true)]
    public FilterDefinition Filter { get; set; } = new();

    public List<ContactResponse> Contacts { get; set; } = new();

    public List<ClassRecord> ClassSelectionList { get; set; } = new();


    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostFilter(CancellationToken cancellationToken)
    {
        return await PreparePage(cancellationToken);
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
                RedirectPath = _linkGenerator.GetPathByPage("/Covers/Index", values: new { area = "ShortTerm" })
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
            // uh oh!
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
    }
}
