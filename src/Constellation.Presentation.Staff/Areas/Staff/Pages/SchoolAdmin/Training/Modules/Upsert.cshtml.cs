namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Modules;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.CreateTrainingModule;
using Constellation.Application.Training.GetTrainingModuleEditContext;
using Constellation.Application.Training.UpdateTrainingModule;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Modules;
    [ViewData] public string PageTitle => Id.Equals(TrainingModuleId.Empty) ? "New Training Module" : "Edit Training Module";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public TrainingModuleId Id { get; set; }

    [BindProperty]
    [Required]
    public string Name { get; set; }

    [BindProperty]
    public TrainingModuleExpiryFrequency Expiry { get; set; }

    [BindProperty]
    public string? ModelUrl { get; set; }

    public async Task OnGet()
    {
        if (!Id.Equals(TrainingModuleId.Empty))
        {
            // Get existing entry from database and populate fields
            Result<ModuleEditContextDto> entityRequest = await _mediator.Send(new GetTrainingModuleEditContextQuery(Id));

            if (entityRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    DomainErrors.Permissions.Unauthorised,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

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
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Update existing entry
        UpdateTrainingModuleCommand command = new(
            Id,
            Name,
            Expiry,
            ModelUrl);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage("Index");
    }
}
