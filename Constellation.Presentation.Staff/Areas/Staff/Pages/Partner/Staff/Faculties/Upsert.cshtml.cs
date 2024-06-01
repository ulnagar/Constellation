namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Constellation.Application.Faculties.CreateFaculty;
using Constellation.Application.Faculties.GetFaculty;
using Constellation.Application.Faculties.UpdateFaculty;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Faculty.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;

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

        return RedirectToPage("/Partner/Staff/Faculties/Index", new { area = "Staff"});
    }
}
