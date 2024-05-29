namespace Constellation.Presentation.Staff.Areas.SchoolAdmin.Pages.Training.Modules;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Modules.CreateTrainingModule;
using Constellation.Application.Training.Modules.GetTrainingModuleEditContext;
using Constellation.Application.Training.Modules.UpdateTrainingModule;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }
    [BindProperty]
    [Required]
    public string Name { get; set; }
    [BindProperty]
    public TrainingModuleExpiryFrequency Expiry { get; set; }
    [BindProperty]
    public string ModelUrl { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Modules;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (Id.HasValue)
        {
            // Get existing entry from database and populate fields
            Result<ModuleEditContextDto> entityRequest = await _mediator.Send(new GetTrainingModuleEditContextQuery(TrainingModuleId.FromValue(Id.Value)));

            if (entityRequest.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = DomainErrors.Permissions.Unauthorised,
                    RedirectPath = _linkGenerator.GetPathByPage("/Training/Modules/Index", values: new { area = "SchoolAdmin" })
                };

                return;
            }

            ModuleEditContextDto entity = entityRequest.Value;

            Name = entity.Name;
            Expiry = entity.Expiry;
            ModelUrl = entity.Url;
        }
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

            return Page();
        }

        // Create new entry
        CreateTrainingModuleCommand command = new(
            Name,
            Expiry,
            ModelUrl);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

            Error = new ErrorDisplay
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

            return Page();
        }

        // Update existing entry
        UpdateTrainingModuleCommand command = new UpdateTrainingModuleCommand(
            TrainingModuleId.FromValue(Id!.Value),
            Name,
            Expiry,
            ModelUrl);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

            Error = new ErrorDisplay
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        return RedirectToPage("Index");
    }
}
