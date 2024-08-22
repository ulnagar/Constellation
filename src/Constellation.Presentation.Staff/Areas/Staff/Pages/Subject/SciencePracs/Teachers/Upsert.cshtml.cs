namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Teachers;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Application.SchoolContacts.GetContactSummary;
using Constellation.Application.SchoolContacts.UpdateContact;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Teachers;
    [ViewData] public string PageTitle { get; set; } = "New Science Prac Teacher";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public SchoolContactId Id { get; set; } = SchoolContactId.Empty;

    [BindProperty]
    [Required]
    public string FirstName { get; set; }

    [BindProperty]
    [Required]
    public string LastName { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    [BindProperty]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    [BindProperty]
    public string SchoolCode { get; set; }

    [BindProperty]
    public string Role { get; set; }

    public SelectList SchoolList { get; set; }
    
    public async Task OnGet()
    {
        if (Id != SchoolContactId.Empty)
        {
            _logger.Information("Requested to retrieve details of School Contact with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            Result<ContactSummaryResponse> contactRequest = await _mediator.Send(new GetContactSummaryQuery(Id));
            
            if (contactRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), contactRequest.Error, true)
                    .Warning("Failed to retrieve details of School Contact with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    contactRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/SciencePracs/Teachers/Index", values: new { area = "Staff" }));

                return;
            }

            FirstName = contactRequest.Value.FirstName;
            LastName = contactRequest.Value.LastName;
            EmailAddress = contactRequest.Value.EmailAddress;
            PhoneNumber = contactRequest.Value.PhoneNumber;

            PageTitle = $"Edit - {FirstName} {LastName}";
        }

        await PreparePage();
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (string.IsNullOrWhiteSpace(SchoolCode))
            ModelState.AddModelError("SchoolCode", "You must select a school");

        if (!ModelState.IsValid)
        {
            await PreparePage();

            return Page();
        }

        CreateContactWithRoleCommand command = new(
            FirstName,
            LastName,
            EmailAddress,
            PhoneNumber,
            Role,
            SchoolCode,
            string.Empty,
            false);

        _logger
            .ForContext(nameof(CreateContactWithRoleCommand), command, true)
            .Information("Requested to create School Contact by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to create School Contact by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Teachers/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            await PreparePage();

            return Page();
        }

        UpdateContactCommand command = new(
            Id,
            FirstName,
            LastName,
            EmailAddress,
            PhoneNumber);

        _logger
            .ForContext(nameof(UpdateContactCommand), command, true)
            .Information("Requested to update School Contact by user {User}", _currentUserService);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to update School Contact by user {User}", _currentUserService);

            ModalContent = new ErrorDisplay(request.Error);

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Teachers/Index", new { area = "Staff" });
    }

    private async Task PreparePage()
    {
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schoolsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), schoolsRequest.Error, true)
                .Warning("Failed to retrieve defaults for School Contact by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                schoolsRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/SciencePracs/Teachers/Index", values: new { area = "Staff" }));

            return;
        }

        SchoolList = new SelectList(schoolsRequest.Value, "Code", "Name");
    }
}
