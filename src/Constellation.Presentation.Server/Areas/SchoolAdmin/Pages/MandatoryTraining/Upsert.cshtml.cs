using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining;

[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.MandatoryTrainingEditor)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpsertModel(IMediator mediator, IDateTimeProvider dateTimeProvider)
    {
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }
    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public string Expiry { get; set; }
    [BindProperty]
    public string ModelUrl { get; set; }

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
        }
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (Id.HasValue)
        {
            // Update existing entry

            var command = new UpdateTrainingModuleCommand
            {
                Id = Id.Value,
                Name = Name,
                Expiry = Expiry,
                Url = ModelUrl,
                ModifiedBy = Request.HttpContext.User.Identity?.Name,
                ModifiedAt = _dateTimeProvider.Now,
            };

            await _mediator.Send(command);
        } else
        {
            // Create new entry

            var command = new CreateTrainingModuleCommand
            {
                Name = Name,
                Expiry = Expiry,
                Url = ModelUrl,
                CreatedBy = Request.HttpContext.User.Identity?.Name,
                CreatedAt = _dateTimeProvider.Now,
            };

            await _mediator.Send(command);
        }

        return RedirectToPage("Index");
    }
}
