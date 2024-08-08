namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Application.Helpers;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Families.GetParentEditContext;
using Constellation.Application.Families.UpdateParent;
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
public class EditParentModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public EditParentModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<EditParentModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle { get; set; } = "Edit Parent";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public ParentId ParentId { get; set; }

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
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

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve Parent with id {Id} for edit by user {User}", ParentId, _currentUserService.UserName);

        Result<ParentEditContextResponse> parentResult = await _mediator.Send(new GetParentEditContextQuery(FamilyId, ParentId), cancellationToken);

        if (parentResult.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                parentResult.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), parentResult.Error, true)
                .Warning("Failed to retrieve Parent with id {Id} for edit by user {User}", ParentId, _currentUserService.UserName);

            return Page();
        }

        Title = parentResult.Value.Title;
        FirstName = parentResult.Value.FirstName;
        LastName = parentResult.Value.LastName;
        MobileNumber = parentResult.Value.MobileNumber;
        EmailAddress = parentResult.Value.EmailAddress;

        PageTitle = $"Edit - {Title} {FirstName} {LastName}";

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        UpdateParentCommand command = new(
            ParentId,
            FamilyId,
            Title,
            FirstName,
            LastName,
            MobileNumber,
            EmailAddress);

        _logger
            .ForContext(nameof(UpdateParentCommand), command, true)
            .Information("Requested to update Parent with id {Id} by user {User}", ParentId, _currentUserService.UserName);

        Result<Parent> result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Index", new { area = "Staff" });

        ModalContent = new ErrorDisplay(
            result.Error,
            _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

        _logger
            .ForContext(nameof(Error), result.Error, true)
            .Warning("Failed to update Parent with id {Id} by user {User}", ParentId, _currentUserService.UserName);

        return Page();
    }
}
