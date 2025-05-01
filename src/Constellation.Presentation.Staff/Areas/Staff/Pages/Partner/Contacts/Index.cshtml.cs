namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Contacts;

using Application.Common.PresentationModels;
using Application.Domains.Contacts.Models;
using Application.Domains.Contacts.Queries.ExportContactList;
using Application.Domains.Contacts.Queries.GetContactList;
using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetCurrentPartnerSchoolsWithStudentsList;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffLinkedToOffering;
using Application.DTOs;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingsForSelectionList;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Offerings.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
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

        AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.IsExecutive);

        ExportContactListCommand command = new(
            offeringIds,
            Filter.Grades,
            Filter.Schools,
            filterCategories,
            execMemberTest.Succeeded);

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
            AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.IsExecutive);

            Result<List<ContactResponse>> contactRequest = await _mediator.Send(
                new GetContactListQuery(
                    offeringIds,
                    Filter.Grades,
                    Filter.Schools,
                    filterCategories,
                    execMemberTest.Succeeded),
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
