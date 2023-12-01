namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

using Constellation.Application.MandatoryTraining.CreateTrainingModule;
using Constellation.Application.MandatoryTraining.GetTrainingModuleEditContext;
using Constellation.Application.MandatoryTraining.UpdateTrainingModule;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.MandatoryTraining.Identifiers;
using Constellation.Presentation.Server.BaseModels;
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

    public async Task OnGet()
    {

        ViewData["ActivePage"] = "Modules";
        ViewData["StaffId"] = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            // Get existing entry from database and populate fields
            var entityRequest = await _mediator.Send(new GetTrainingModuleEditContextQuery(TrainingModuleId.FromValue(Id.Value)));

            if (entityRequest.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = DomainErrors.Permissions.Unauthorised,
                    RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
                };

                return;
            }

            var entity = entityRequest.Value;

            Name = entity.Name;
            Expiry = entity.Expiry;
            ModelUrl = entity.Url;
            CanMarkNotRequired = entity.CanMarkNotRequired;
        }
    }

    public async Task<IActionResult> OnPostUpdate()
    {

        ViewData["ActivePage"] = "Modules";
        ViewData["StaffId"] = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (Id.HasValue)
        {
            // Update existing entry
            var command = new UpdateTrainingModuleCommand(
                TrainingModuleId.FromValue(Id.Value),
                Name,
                Expiry,
                ModelUrl,
                CanMarkNotRequired);

            var result = await _mediator.Send(command);

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
            var command = new CreateTrainingModuleCommand(
                Name,
                Expiry,
                ModelUrl,
                CanMarkNotRequired);

            var result = await _mediator.Send(command);

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
