namespace Constellation.Presentation.Server.Areas.Partner.Pages.Faculties;

using Application.Faculties.CreateFaculty;
using Application.Faculties.GetFaculty;
using Application.Faculties.UpdateFaculty;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Faculty.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditFaculties)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;

    public UpsertModel(ISender mediator)
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
        if (Id.HasValue)
        {
            FacultyId facultyId = FacultyId.FromValue(Id.Value);
            
            // Get values from database
            Result<FacultyResponse> request = await _mediator.Send(new GetFacultyQuery(facultyId));

            if (request.IsFailure)
            {
                Error = new()
                {
                    Error = request.Error,
                    RedirectPath = null
                };

                return;
            }

            Name = request.Value.Name;
            Colour = request.Value.Colour;
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
            FacultyId facultyId = FacultyId.FromValue(Id.Value);

            UpdateFacultyCommand command = new(
                facultyId,
                Name,
                Colour);

            await _mediator.Send(command);
        }
        else
        {
            // Create new entry

            CreateFacultyCommand command = new(
                Name,
                Colour);

            await _mediator.Send(command);
        }

        return RedirectToPage("Index");
    }
}
