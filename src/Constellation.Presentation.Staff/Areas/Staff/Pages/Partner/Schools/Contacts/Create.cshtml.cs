namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools.Contacts;

using Application.Common.PresentationModels;
using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContact;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
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
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanManageSchoolContacts)]
public class CreateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public CreateModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<CreateModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Contacts;
    [ViewData] public string PageTitle => "Create School Contact";

    [BindProperty]
    [Required]
    [Display(Name = DisplayNameDefaults.FirstName)]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = DisplayNameDefaults.LastName)]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = DisplayNameDefaults.EmailAddress)]
    public string EmailAddress { get; set; } = string.Empty;

    [BindProperty]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = DisplayNameDefaults.PhoneNumber)]
    public string? PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    public string? SchoolCode { get; set; }
    [BindProperty]
    public string? Role { get; set; }
    [BindProperty]
    public string? Note { get; set; }

    public SelectList SchoolsList { get; set; }
    public SelectList RolesList { get; set; }

    public async Task OnGet()
    {
        Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
        if (rolesRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                rolesRequest.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), rolesRequest.Error, true)
                .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

            return;
        }
        RolesList = new SelectList(rolesRequest.Value);
        
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));
        if (schoolsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                schoolsRequest.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), schoolsRequest.Error, true)
                .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

            return;
        }
        SchoolsList = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
            if (rolesRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    rolesRequest.Error,
                    _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

                _logger
                    .ForContext(nameof(Error), rolesRequest.Error, true)
                    .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

                return Page();
            }
            RolesList = new SelectList(rolesRequest.Value);

            Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));
            if (schoolsRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    schoolsRequest.Error,
                    _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

                _logger
                    .ForContext(nameof(Error), schoolsRequest.Error, true)
                    .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

                return Page();
            }
            SchoolsList = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");

            return Page();
        }

        FirstName = FirstName.Trim();
        LastName = LastName.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? PhoneNumber : PhoneNumber.Trim();
        EmailAddress = EmailAddress.Trim();
        
        Role = string.IsNullOrWhiteSpace(Role) ? string.Empty : Role.Trim();
        Note = string.IsNullOrWhiteSpace(Note) ? Note : Note.Trim();
        
        if (string.IsNullOrWhiteSpace(SchoolCode))
        {
            CreateContactCommand createCommand = new(
                FirstName,
                LastName,
                EmailAddress,
                PhoneNumber,
                false);

            _logger
                .ForContext(nameof(CreateContactCommand), createCommand, true)
                .Information("Requested to create School Contact by user {User}", _currentUserService.UserName);

            Result<SchoolContactId> request = await _mediator.Send(createCommand);

            if (request.IsFailure)
            {
                ModalContent = new ErrorDisplay(request.Error);

                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to create School Contact by user {User}", _currentUserService.UserName);

                Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
                if (rolesRequest.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        rolesRequest.Error,
                        _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

                    _logger
                        .ForContext(nameof(Error), rolesRequest.Error, true)
                        .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

                    return Page();
                }
                RolesList = new SelectList(rolesRequest.Value);

                Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));
                if (schoolsRequest.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        schoolsRequest.Error,
                        _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

                    _logger
                        .ForContext(nameof(Error), schoolsRequest.Error, true)
                        .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

                    return Page();
                }
                SchoolsList = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");

                return Page();
            }
        }
        else
        {
            CreateContactWithRoleCommand createWithRoleCommand = new(
                FirstName,
                LastName,
                EmailAddress,
                PhoneNumber,
                Role,
                SchoolCode,
                Note,
                false);

            _logger
                .ForContext(nameof(CreateContactWithRoleCommand), createWithRoleCommand, true)
                .Information("Requested to create new School Contact by user {User}", _currentUserService.UserName);

            Result request = await _mediator.Send(createWithRoleCommand);

            if (request.IsFailure)
            {
                ModalContent = new ErrorDisplay(request.Error);

                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

                Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
                if (rolesRequest.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        rolesRequest.Error,
                        _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

                    _logger
                        .ForContext(nameof(Error), rolesRequest.Error, true)
                        .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

                    return Page();
                }
                RolesList = new SelectList(rolesRequest.Value);

                Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));
                if (schoolsRequest.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        schoolsRequest.Error,
                        _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

                    _logger
                        .ForContext(nameof(Error), schoolsRequest.Error, true)
                        .Warning("Failed to create new School Contact by user {User}", _currentUserService.UserName);

                    return Page();
                }
                SchoolsList = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");

                return Page();
            }
        }
       
        return RedirectToPage("/Partner/Schools/Contacts/Index", new { area = "Staff" });
    }
}