namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;

    public UpsertModel(IMediator mediator)
    {
        _mediator = mediator;
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
        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            // Get existing entry from database and populate fields

            var entity = await _mediator.Send(new GetTrainingModuleEditContextQuery { Id = Id.Value });

            //TODO: Check that the return value is not null
            // If it is, redirect and show error message?

            Name = entity.Name;
            Expiry = entity.Expiry;
            ModelUrl = entity.Url;
            CanMarkNotRequired = entity.CanMarkNotRequired;
        }
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (Id.HasValue)
        {
            // Update existing entry

            var command = new UpdateTrainingModuleCommand(
                Id.Value,
                Name,
                Expiry,
                ModelUrl,
                CanMarkNotRequired);

            await _mediator.Send(command);
        }
        else
        {
            // Create new entry

            var command = new CreateTrainingModuleCommand(
                Name,
                Expiry,
                ModelUrl,
                CanMarkNotRequired);

            await _mediator.Send(command);
        }

        return RedirectToPage("Index");
    }
}
