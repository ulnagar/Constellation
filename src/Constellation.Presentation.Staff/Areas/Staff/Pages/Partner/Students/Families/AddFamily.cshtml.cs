namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Constellation.Application.Domains.Families.Commands.CreateFamily;
using Constellation.Application.Models.Auth;
using Constellation.Core.ValueObjects;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Families;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class AddFamilyModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddFamilyModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddFamilyModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle => "Add New Family";

    [BindProperty]
    [Display(Name = "Family Title")]
    [Required]
    public string FamilyTitle { get; set; } = string.Empty;
    
    [BindProperty]
    [Required]
    [Display(Name = "Address Line 1")]
    public string AddressLine1 { get; set; } = string.Empty;
    
    [BindProperty]
    [Display(Name = "Address Line 2")]
    public string? AddressLine2 { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = "Town")]
    public string AddressTown { get; set; } = string.Empty;
    
    [BindProperty]
    [Required]
    [Display(Name = "Post Code")]
    public string AddressPostCode { get; set; } = string.Empty;
    
    [BindProperty]
    [EmailAddress]
    [Required]
    [Display(Name = "Family Email Address")]
    public string FamilyEmail { get; set; } = string.Empty;

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAdd(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        Result<EmailAddress> email = EmailAddress.Create(FamilyEmail);

        if (email.IsFailure)
        {
            ModelState.AddModelError("FamilyEmail", email.Error.Message);

            return Page();
        }

        CreateFamilyCommand command = new(
            FamilyTitle,
            AddressLine1,
            AddressLine2,
            AddressTown,
            AddressPostCode,
            email.Value);

        _logger
            .ForContext(nameof(CreateFamilyCommand), command, true)
            .Information("Requested to create new Family by user {User}", _currentUserService.UserName);

        Result<Family> result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Details", new { area = "Staff", Id = result.Value.Id.Value });

        ModalContent = new ErrorDisplay(
            result.Error,
            _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));
        
        _logger
            .ForContext(nameof(Error), result.Error, true)
            .Information("Failed to create new Family by user {User}", _currentUserService.UserName);

        return Page();
    }
}
