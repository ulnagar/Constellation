namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

using Application.Training.Modules.CreateTrainingModule;
using Application.Training.Modules.GetTrainingModuleEditContext;
using Application.Training.Modules.UpdateTrainingModule;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
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
    public string Name { get; set; }
    [BindProperty]
    public TrainingModuleExpiryFrequency Expiry { get; set; }
    [BindProperty]
    public string ModelUrl { get; set; }
    [BindProperty]
    public bool CanMarkNotRequired { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Modules;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            // Get existing entry from database and populate fields
            Result<ModuleEditContextDto> entityRequest = await _mediator.Send(new GetTrainingModuleEditContextQuery(TrainingModuleId.FromValue(Id.Value)));

            if (entityRequest.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = DomainErrors.Permissions.Unauthorised,
                    RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
                };

                return;
            }

            ModuleEditContextDto entity = entityRequest.Value;

            Name = entity.Name;
            Expiry = entity.Expiry;
            ModelUrl = entity.Url;
            CanMarkNotRequired = entity.CanMarkNotRequired;
        }
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (Id.HasValue)
        {
            // Update existing entry
            UpdateTrainingModuleCommand command = new UpdateTrainingModuleCommand(
                TrainingModuleId.FromValue(Id.Value),
                Name,
                Expiry,
                ModelUrl,
                CanMarkNotRequired);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
                };

                return Page();
            }
        }
        else
        {
            // Create new entry
            CreateTrainingModuleCommand command = new CreateTrainingModuleCommand(
                Name,
                Expiry,
                ModelUrl,
                CanMarkNotRequired);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
                };

                return Page();
            }
        }

        return RedirectToPage("Index");
    }
}
