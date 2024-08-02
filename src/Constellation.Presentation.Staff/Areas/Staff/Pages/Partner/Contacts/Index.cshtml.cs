namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Contacts;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Models.Auth;
using Application.Offerings.GetOfferingsForSelectionList;
using Application.StaffMembers.GetStaffLinkedToOffering;
using Application.StaffMembers.Models;
using Areas;
using Constellation.Application.Contacts.ExportContactList;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.Contacts.Models;
using Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Offerings.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Contacts_List;
    [ViewData] public string PageTitle => "Contacts List";

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

        ExportContactListCommand command = new(
            offeringIds,
            Filter.Grades,
            Filter.Schools,
            filterCategories);

        _logger
            .ForContext(nameof(ExportContactListCommand), command, true)
            .Information("Requested to export contact list by user {User}", _currentUserService.UserName);

        Result<FileDto> file = await _mediator.Send(command, cancellationToken);
        
        if (file.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                file.Error,
                _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" }));

            _logger
                .ForContext(nameof(Error), file.Error, true)
                .Warning("Failed to export contact list by user {User}", _currentUserService.UserName);

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                classesResponse.Error,
                _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" }));

            _logger
                .ForContext(nameof(Error), classesResponse.Error, true)
                .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

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
            ModalContent = new ErrorDisplay(
                schoolsRequest.Error,
                _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" }));


            _logger
                .ForContext(nameof(Error), schoolsRequest.Error, true)
                .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

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
                ModalContent = new ErrorDisplay(
                    contactRequest.Error,
                    _linkGenerator.GetPathByPage("/Contacts/Index", values: new { area = "Partner" }));

                _logger
                    .ForContext(nameof(Error), contactRequest.Error, true)
                    .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

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
