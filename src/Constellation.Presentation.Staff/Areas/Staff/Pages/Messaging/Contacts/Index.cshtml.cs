namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Messaging.Contacts;

using Application.Common.PresentationModels;
using Application.Domains.Contacts.Interfaces;
using Application.Domains.Contacts.Models;
using Application.Domains.Contacts.Queries.ExportContactList;
using Application.Domains.Contacts.Queries.GetContactList;
using Application.Domains.Courses.Models;
using Application.Domains.Courses.Queries.GetCoursesForSelectionList;
using Application.Domains.SchoolContacts.Commands.UpdateSchoolContactPhoneNumber;
using Application.Domains.SchoolContacts.Queries.GetContactSummary;
using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetCurrentPartnerSchoolsWithStudentsList;
using Application.Domains.StaffMembers.Commands.UpdateStaffMemberPhoneNumber;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffById;
using Application.Domains.StaffMembers.Queries.GetStaffLinkedToOffering;
using Application.DTOs;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingsForSelectionList;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Drafts.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using Shared.PartialViews.AddPhoneNumberToSchoolContact;
using Shared.PartialViews.AddPhoneNumberToStaffMember;

[HasPermission(AuthPermission.Messaging_Contacts_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IStudentFlagCacheService _flagCache;
    private readonly IMessageDraftRepository _draftRepository;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IStudentFlagCacheService flagCache,
        IMessageDraftRepository draftRepository,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger logger)
    {
        _mediator = mediator;
        _flagCache = flagCache;
        _draftRepository = draftRepository;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Messaging_Contacts_List;
    [ViewData] public string PageTitle => "Contacts List";

    [BindProperty]
    public FilterDefinition Filter { get; set; } = new();

    public List<ContactResponse> Contacts { get; set; } = [];

    public List<ClassRecord> ClassSelectionList { get; set; } = [];

    public List<CourseSelectListItemResponse> CourseSelectionList { get; set; } = [];

    public List<SchoolSelectionListResponse> SchoolsList { get; set; } = [];

    public List<string> Flags { get; set; } = [];

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
        List<ContactCategory> filterCategories = [];

        foreach (string entry in Filter.Categories)
            filterCategories.Add(ContactCategory.FromValue(entry));

        List<OfferingId> offeringIds = Filter.Offerings.Select(OfferingId.FromValue).ToList();
        List<CourseId> courseIds = Filter.Courses.Select(CourseId.FromValue).ToList();

        AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPermission.Partners_SchoolContacts_ShowPrincipals_Value);

        ExportContactListCommand command = new(
            offeringIds,
            courseIds,
            Filter.Grades,
            Filter.Schools,
            filterCategories,
            Filter.Flags,
            execMemberTest.Succeeded);

        _logger
            .ForContext(nameof(ExportContactListCommand), command, true)
            .Information("Requested to export contact list by user {User}", _currentUserService.UserName);

        Result<FileDto> file = await _mediator.Send(command, cancellationToken);
        
        if (file.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                file.Error,
                _linkGenerator.GetPathByPage("/Messaging/Contacts/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), file.Error, true)
                .Warning("Failed to export contact list by user {User}", _currentUserService.UserName);

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }

    public async Task<IActionResult> OnPostAddRecipients(List<MessageRecipient> recipients)
    {
        foreach (var recipient in recipients)
            await _draftRepository.AddRecipient(recipient, User.GetUserId());

        return new OkResult();
    }

    public async Task<IActionResult> OnPostAjaxContactPhoneUpdate(SchoolContactId contactId)
    {
        Result<ContactSummaryResponse> contact = await _mediator.Send(new GetContactSummaryQuery(contactId));

        if (contact.IsFailure)
            return Content(string.Empty);

        AddPhoneNumberToSchoolContactViewModel viewModel = new()
        {
            ContactId = contact.Value.ContactId,
            PhoneNumber = contact.Value.PhoneNumber
        };

        return Partial("AddPhoneNumberToSchoolContact", viewModel);
    }

    public async Task<IActionResult> OnPostAjaxStaffPhoneUpdate(StaffId staffId)
    {
        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(staffId));

        if (staffMember.IsFailure)
            return Content(string.Empty);

        AddPhoneNumberToStaffMemberViewModel viewModel = new()
        {
            StaffId = staffMember.Value.StaffId,
            PhoneNumber = staffMember.Value.PhoneNumber
        };

        return Partial("AddPhoneNumberToStaffMember", viewModel);
    }

    public async Task<IActionResult> OnPostStaffPhoneUpdate(StaffId staffId, [ModelBinder(typeof(FromValueBinder))] PhoneNumber phoneNumber)
    {
        if (phoneNumber == PhoneNumber.Empty)
            return new JsonResult(new { success = false, error = "Phone number is required." });

        Result update = await _mediator.Send(new UpdateStaffMemberPhoneNumberCommand(staffId, phoneNumber));

        if (update.IsFailure)
            return new JsonResult(new { success = false, error = update.Error.Message });

        return new JsonResult(new { success = true, phoneNumber = phoneNumber.ToString() });
    }

    public async Task<IActionResult> OnPostContactPhoneUpdate(SchoolContactId contactId, [ModelBinder(typeof(FromValueBinder))] PhoneNumber phoneNumber)
    {
        if (phoneNumber == PhoneNumber.Empty)
            return new JsonResult(new { success = false, error = "Phone number is required." });

        Result update = await _mediator.Send(new UpdateSchoolContactPhoneNumberCommand(contactId, phoneNumber));

        if (update.IsFailure)
            return new JsonResult(new { success = false, error = update.Error.Message });

        return new JsonResult(new { success = true, phoneNumber = phoneNumber.ToString() });
    }

    public async Task<IActionResult> OnPostFilterPartial(CancellationToken cancellationToken)
    {
        List<ContactCategory> filterCategories = Filter
            .Categories
            .Select(ContactCategory.FromValue)
            .ToList();

        List<OfferingId> offeringIds = Filter.Offerings.Select(OfferingId.FromValue).ToList();
        List<CourseId> courseIds = Filter.Courses.Select(CourseId.FromValue).ToList();

        if (!offeringIds.Any() 
            && !courseIds.Any() 
            && !filterCategories.Any()
            && !Filter.Grades.Any() 
            && !Filter.Schools.Any() 
            && !Filter.Flags.Any())
            return Partial("ContactsTable", (new List<ContactResponse>(), false));

        AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPermission.Partners_SchoolContacts_ShowPrincipals_Value);

        Result<List<ContactResponse>> contactRequest = await _mediator.Send(
            new GetContactListQuery(
                offeringIds,
                courseIds,
                Filter.Grades,
                Filter.Schools,
                filterCategories,
                Filter.Flags,
                execMemberTest.Succeeded),
            cancellationToken);

        if (contactRequest.IsFailure)
            return new JsonResult(new { success = false, error = contactRequest.Error.Message });

        List<ContactResponse> contacts = contactRequest
            .Value
            .OrderBy(c => c.StudentGrade)
            .ThenBy(c => c.Student.LastName)
            .ThenBy(c => c.Student.FirstName)
            .ToList();

        return Partial("ContactsTable", (contacts, execMemberTest));
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken = default)
    {
        Result<List<CourseSelectListItemResponse>> coursesResponse = await _mediator.Send(new GetCoursesForSelectionListQuery(true));

        if (coursesResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                coursesResponse.Error,
                _linkGenerator.GetPathByPage("/Messaging/Contacts/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), coursesResponse.Error, true)
                .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

            return Page();
        }

        CourseSelectionList = coursesResponse.Value;

        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                classesResponse.Error,
                _linkGenerator.GetPathByPage("/Messaging/Contacts/Index", values: new { area = "Staff" }));

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

        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetCurrentPartnerSchoolsWithStudentsListQuery(), cancellationToken);

        if (schoolsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                schoolsRequest.Error,
                _linkGenerator.GetPathByPage("/Messaging/Contacts/Index", values: new { area = "Staff" }));


            _logger
                .ForContext(nameof(Error), schoolsRequest.Error, true)
                .Warning("Failed to retrieve contact list by user {User}", _currentUserService.UserName);

            return Page();
        }

        SchoolsList = schoolsRequest.Value;

        List<ContactCategory> filterCategories = [];

        foreach (string entry in Filter.Categories)
            filterCategories.Add(ContactCategory.FromValue(entry));

        Flags = await _flagCache.GetFlags();

        List<OfferingId> offeringIds = Filter.Offerings.Select(OfferingId.FromValue).ToList();
        List<CourseId> courseIds = Filter.Courses.Select(CourseId.FromValue).ToList();

        if (offeringIds.Any() ||
            courseIds.Any() ||
            filterCategories.Any() ||
            Filter.Grades.Any() ||
            Filter.Schools.Any() ||
            Filter.Flags.Any())
        {
            AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPermission.Partners_SchoolContacts_ShowPrincipals_Value);

            Result<List<ContactResponse>> contactRequest = await _mediator.Send(
                new GetContactListQuery(
                    offeringIds,
                    courseIds,
                    Filter.Grades,
                    Filter.Schools,
                    filterCategories,
                    Filter.Flags,
                    execMemberTest.Succeeded),
                cancellationToken);

            if (contactRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(
                    contactRequest.Error,
                    _linkGenerator.GetPathByPage("/Messaging/Contacts/Index", values: new { area = "Staff" }));

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
        public List<Guid> Offerings { get; set; } = [];
        public List<Grade> Grades { get; set; } = [];
        public List<SchoolCode> Schools { get; set; } = [];
        public List<string> Categories { get; set; } = [];
        public List<string> Flags { get; set; } = [];
        public List<Guid> Courses { get; set; } = [];

        public FilterAction Action { get; set; } = FilterAction.Filter;

        public enum FilterAction
        {
            Filter,
            Export,
            Email
        }
    }
}
