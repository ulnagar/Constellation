namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools.Contacts;

using Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContactRoleAssignment;
using Constellation.Application.SchoolContacts.ExportContactsBySchool;
using Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;
using Constellation.Application.SchoolContacts.GetContactsBySchool;
using Constellation.Application.SchoolContacts.RemoveContactRole;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Application.Users.RepairSchoolContactUser;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using Shared.PartialViews.AssignRoleModal;
using Shared.PartialViews.DeleteRoleModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ReportsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ReportsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ReportsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_ContactReports;
    [ViewData] public string PageTitle => "School Contact Reports";

    public List<SchoolWithContactsResponse> Schools { get; set; } = new();

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve School Contact Reports by user {User}", _currentUserService.UserName);

        Result<List<SchoolWithContactsResponse>> schools = await _mediator.Send(new GetContactsBySchoolQuery());

        if (schools.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                schools.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff"}));

            _logger
                .ForContext(nameof(Error), schools.Error, true)
                .Warning("Failed to retrieve School Contact Reports by user {User}", _currentUserService.UserName);

            return;
        }

        Schools = schools.Value.OrderBy(entry => entry.SchoolName).ToList();
    }

    public async Task<IActionResult> OnGetExport()
    {
        _logger
            .Information("Requested to export School Contact Reports by user {User}", _currentUserService.UserName);

        Result<FileDto> file = await _mediator.Send(new ExportContactsBySchoolQuery());

        if (file.IsFailure)
        {
            ModalContent = new ErrorDisplay(file.Error);

            _logger
                .ForContext(nameof(Error), file.Error, true)
                .Warning("Failed to export School Contact Reports by user {User}", _currentUserService.UserName);

            Result<List<SchoolWithContactsResponse>> schools = await _mediator.Send(new GetContactsBySchoolQuery());
            Schools = schools.Value.OrderBy(entry => entry.SchoolName).ToList();
            
            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }

    public async Task<IActionResult> OnPostAjaxAssign(
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        string name)
    {
        AssignRoleModalViewModel viewModel = new();

        Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        viewModel.Schools = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
        viewModel.Roles = new SelectList(rolesRequest.Value);
        viewModel.ContactId = contactId;
        viewModel.ContactName = name;

        return Partial("AssignRoleModal", viewModel);
    }

    public async Task<IActionResult> OnPostCreateAssignment(
        string schoolCode,
        string roleName,
        string note,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId)
    {
        CreateContactRoleAssignmentCommand command = new(contactId, schoolCode, roleName, note);

        _logger
            .ForContext(nameof(CreateContactRoleAssignmentCommand), command, true)
            .Information("Requested to create assignment for School Contact by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to create assignment for School Contact by user {User}", _currentUserService.UserName);

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxDelete(
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactRoleId roleId,
        string name,
        string role,
        string school)
    {
        DeleteRoleModalViewModel viewModel = new(
            contactId,
            roleId,
            name,
            role,
            school);

        return Partial("DeleteRoleModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteAssignment(
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactRoleId roleId)
    {
        RemoveContactRoleCommand command = new(contactId, roleId);

        _logger
            .ForContext(nameof(RemoveContactRoleCommand), command, true)
            .Information("Requested to remove assignment for School Contact by user {User}", _currentUserService.UserName);

        await _mediator.Send(command);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRepairUser([ModelBinder(typeof(StrongIdBinder))] SchoolContactId id)
    {
        RepairSchoolContactUserCommand command = new(id);

        _logger
            .ForContext(nameof(RepairSchoolContactUserCommand), command, true)
            .Information("Requested to repair user account for School Contact by user {User}", _currentUserService.UserName);

        await _mediator.Send(command);

        return RedirectToPage();
    }
}