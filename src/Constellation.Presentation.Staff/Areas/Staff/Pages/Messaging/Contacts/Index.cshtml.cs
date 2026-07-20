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
using Application.Domains.StaffMembers.Queries.GetStaffById;
using Application.Domains.StaffMembers.Queries.GetStaffLinkedToAllOfferings;
using Application.DTOs;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingsForSelectionList;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Drafts.Repositories;
using Core.Models.Messaging.EmergencyConsole.Services;
using Core.Models.Messaging.Enums;
using Core.Models.Offerings.Identifiers;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Extensions;
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
    private readonly IEmergencyRecipientService _recipientService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IStudentFlagCacheService flagCache,
        IMessageDraftRepository draftRepository,
        IEmergencyRecipientService recipientService,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger logger)
    {
        _mediator = mediator;
        _flagCache = flagCache;
        _draftRepository = draftRepository;
        _recipientService = recipientService;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Messaging_Contacts_List;
    [ViewData] public string PageTitle => "Contacts List";

    [BindProperty]
    public ContactFilter Filter { get; set; } = new();

    public List<ContactResponse> Contacts { get; set; } = [];

    public List<ClassRecord> ClassSelectionList { get; set; } = [];

    public List<CourseSelectListItemResponse> CourseSelectionList { get; set; } = [];

    public List<SchoolSelectionListResponse> SchoolsList { get; set; } = [];
    
    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        if (Filter.AnyDefined)
        {
            AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPermission.Partners_SchoolContacts_ShowPrincipals_Value);

            Result<List<ContactResponse>> contactRequest = await _mediator.Send(
                new GetContactListQuery(
                    Filter,
                    execMemberTest.Succeeded),
                cancellationToken);

            if (contactRequest.IsFailure)
                return FailPage(contactRequest.Error, "Failed to retrieve contact list");

            Contacts = contactRequest.Value
                .OrderBy(contact => contact.StudentGrade)
                .ThenBy(contact => contact.Student.LastName)
                .ThenBy(contact => contact.Student.FirstName)
                .ToList();
        }

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnGetFlagList()
    {
        List<StudentFlag> flags = await _flagCache.GetFlags();
        return new JsonResult(flags.OrderBy(f => f.Name).Select(f => f.Name));
    }

    public async Task<IActionResult> OnPostFilter(CancellationToken cancellationToken)
    {
        if (Filter.Action == ContactFilter.FilterAction.Filter)
        {
            if (Filter.AnyDefined)
            {
                AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPermission.Partners_SchoolContacts_ShowPrincipals_Value);

                Result<List<ContactResponse>> contactRequest = await _mediator.Send(
                    new GetContactListQuery(
                        Filter,
                        execMemberTest.Succeeded),
                    cancellationToken);

                if (contactRequest.IsFailure)
                    return FailPage(contactRequest.Error, "Failed to retrieve contact list");

                Contacts = contactRequest.Value
                    .OrderBy(contact => contact.StudentGrade)
                    .ThenBy(contact => contact.Student.LastName)
                    .ThenBy(contact => contact.Student.FirstName)
                    .ToList();
            }

            return await PreparePage(cancellationToken);
        }

        if (Filter.Action == ContactFilter.FilterAction.Export)
            return await OnPostExport(cancellationToken);

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostExport(CancellationToken cancellationToken)
    {
        AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPermission.Partners_SchoolContacts_ShowPrincipals_Value);

        ExportContactListCommand command = new(
            Filter,
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

    public async Task<IActionResult> OnPostEmergencyGroup(RecipientGroup group)
    {
        Result<List<ContactResponse>> contactRequest = await _recipientService.GetSelectedRecipientsFromGroup(group);

        if (contactRequest.IsFailure)
            return FailPage(contactRequest.Error, "Failed to retrieve contact list");

        Contacts = contactRequest.Value
            .OrderBy(contact => contact.StudentGrade)
            .ThenBy(contact => contact.Student.LastName)
            .ThenBy(contact => contact.Student.FirstName)
            .ToList();

        return await PreparePage();
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

        AddPhoneNumberToSchoolContactViewModel viewModel = new(contact.Value.PhoneNumber, contact.Value.ContactId);

        return Partial("AddPhoneNumberToSchoolContact", viewModel);
    }

    public async Task<IActionResult> OnPostAjaxStaffPhoneUpdate(StaffId staffId)
    {
        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(staffId));

        if (staffMember.IsFailure)
            return Content(string.Empty);

        AddPhoneNumberToStaffMemberViewModel viewModel = new(staffMember.Value.PhoneNumber, staffMember.Value.StaffId);

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

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken = default)
    {
        Result<List<CourseSelectListItemResponse>> coursesResponse = await _mediator.Send(new GetCoursesForSelectionListQuery(true), cancellationToken);
        if (coursesResponse.IsFailure)
            return FailPage(coursesResponse.Error, "Failed to retrieve course list");

        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);
        if (classesResponse.IsFailure)
            return FailPage(classesResponse.Error, "Failed to retrieve offerings list");

        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetCurrentPartnerSchoolsWithStudentsListQuery(), cancellationToken);
        if (schoolsRequest.IsFailure)
            return FailPage(schoolsRequest.Error, "Failed to retrieve schools list");
        
        CourseSelectionList = coursesResponse.Value;
        SchoolsList = schoolsRequest.Value;

        Result<List<OfferingStaffResponse>> allStaffResponse = await _mediator.Send(new GetStaffLinkedToAllOfferingsQuery(), cancellationToken);

        if (allStaffResponse.IsSuccess)
        {
            ILookup<OfferingId, OfferingStaffResponse> staffByOffering =
                allStaffResponse.Value.ToLookup(x => x.OfferingId);

            foreach (OfferingSelectionListResponse offering in classesResponse.Value)
            {
                IEnumerable<OfferingStaffResponse> teachers = staffByOffering[offering.Id];
                if (!teachers.Any())
                    continue;

                OfferingStaffResponse primaryTeacher = teachers
                    .GroupBy(x => x.StaffId)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .First();
                    
                ClassSelectionList.Add(new ClassRecord(
                    offering.Id,
                    offering.Name,
                    $"{primaryTeacher.Name.PreferredName[..1]} {primaryTeacher.Name.LastName}",
                    $"Year {offering.Name[..2]}"));
            }
        }

        return Page();
    }

    // Helper to reduce the repeated error-handling boilerplate
    private IActionResult FailPage(Error error, string message)
    {
        ModalContent = ErrorDisplay.Create(
            error,
            _linkGenerator.GetPathByPage("/Messaging/Contacts/Index", values: new { area = "Staff" }));

        _logger
            .ForContext(nameof(Error), error, true)
            .Warning("{Message} by user {User}", message, _currentUserService.UserName);

        return Page();
    }
}
