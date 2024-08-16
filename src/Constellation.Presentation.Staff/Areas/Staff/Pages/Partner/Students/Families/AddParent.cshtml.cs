namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Application.Helpers;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Families.CreateParent;
using Core.Abstractions.Services;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class AddParentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddParentModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddParentModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle => "Add New Parent";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public FamilyId FamilyId { get; set; }

    [BindProperty]
    public string? Title { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = DisplayNameDefaults.FirstName)]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = DisplayNameDefaults.LastName)]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [Phone]
    [Display(Name = DisplayNameDefaults.MobileNumber)]
    public string? MobileNumber { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress]
    [Required]
    [Display(Name = DisplayNameDefaults.EmailAddress)]
    public string EmailAddress { get; set; } = string.Empty;

    public IActionResult OnGet()=> Page();

    public async Task<IActionResult> OnPostCreate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) 
            return Page();
        
        CreateParentCommand command = new(
            FamilyId,
            Title,
            FirstName,
            LastName,
            MobileNumber,
            EmailAddress);

        _logger
            .ForContext(nameof(CreateParentCommand), command, true)
            .Information("Requested to create new Parent by user {User}", _currentUserService.UserName);

        Result<Parent> result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Details", new { area = "Staff", Id = FamilyId.Value });

        ModalContent = new ErrorDisplay(
            result.Error,
            _linkGenerator.GetPathByPage("/Partner/Students/Families/Details", values: new { area = "Staff", Id = FamilyId.Value }));

        _logger
            .ForContext(nameof(Error), result.Error, true)
            .Warning("Failed to create new Parent by user {User}", _currentUserService.UserName);

        return Page();
    }
}
