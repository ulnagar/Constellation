namespace Constellation.Presentation.Server.Areas.Partner.Pages.Faculties;

using Constellation.Application.Features.Faculties.Commands;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditFaculties)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;

    public UpsertModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [Required]
    [BindProperty]
    public string Name { get; set; }
    [Required]
    [BindProperty]
    public string Colour { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            // Get values from database
            var entry = await _mediator.Send(new GetFacultyEditContextQuery(Id.Value));

            Name = entry.Name;
            Colour = entry.Colour;
        }
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Id.HasValue)
        {
            // Update existing entry

            var command = new UpdateFacultyCommand(
                Id.Value,
                Name,
                Colour);

            await _mediator.Send(command);
        }
        else
        {
            // Create new entry

            var command = new CreateFacultyCommand(
                Name,
                Colour);

            await _mediator.Send(command);
        }

        return RedirectToPage("Index");
    }
}
