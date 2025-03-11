namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Families.GetFamilyEditContext;
using Constellation.Application.Families.UpdateFamily;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class EditFamilyModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public EditFamilyModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<EditFamilyModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle { get; set; } = "Edit Family";

    [BindProperty(SupportsGet = true)]
    public FamilyId Id { get; set; }

    [BindProperty]
    [Display(Name = "Family Title")]
    [Required]
    public string FamilyTitle { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Address Line 1")]
    public string? AddressLine1 { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Address Line 2")]
    public string? AddressLine2 { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Town")]
    public string? AddressTown { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Post Code")]
    public string? AddressPostCode { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress]
    [Required]
    [Display(Name = "Family Email Address")]
    public string FamilyEmail { get; set; } = string.Empty;

    public List<string> Students { get; set; } = new();
    public List<string> Parents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve Family with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<FamilyEditContextResponse> familyResult = await _mediator.Send(new GetFamilyEditContextQuery(Id), cancellationToken);

        if (familyResult.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                familyResult.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), familyResult.Error, true)
                .Warning("Failed to retrieve Family with id {Id} for edit by user {User}", Id, _currentUserService.UserName);
            
            return Page();
        }

        FamilyTitle = familyResult.Value.FamilyTitle;
        AddressLine1 = familyResult.Value.AddressLine1;
        AddressLine2 = familyResult.Value.AddressLine2;
        AddressTown = familyResult.Value.AddressTown;
        AddressPostCode = familyResult.Value.AddressPostCode;
        FamilyEmail = familyResult.Value.FamilyEmail;
        Students = familyResult.Value.Students;
        Parents = familyResult.Value.Parents;

        PageTitle = $"Edit - {FamilyTitle}";

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        UpdateFamilyCommand command = new(
            Id,
            FamilyTitle,
            AddressLine1,
            AddressLine2,
            AddressTown,
            AddressPostCode,
            FamilyEmail);

        _logger
            .ForContext(nameof(UpdateFamilyCommand), command, true)
            .Information("Requested to update Family with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Index", new { area = "Staff" });

        ModalContent = new ErrorDisplay(
            result.Error,
            _linkGenerator.GetPathByPage("/Partner/Students/Families/EditFamily", values: new { area = "Staff", Id = Id }));


        _logger
            .ForContext(nameof(Error), result.Error, true)
            .Warning("Failed to update Family with id {Id} by user {User}", Id, _currentUserService.UserName);

        return Page();
    }
}
