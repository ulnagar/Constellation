namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.GroupTutorials.Tutorials;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.GroupTutorials.CreateGroupTutorial;
using Constellation.Application.GroupTutorials.EditGroupTutorial;
using Constellation.Application.GroupTutorials.GetTutorialById;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditGroupTutorials)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(ISender mediator, LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_GroupTutorials_Tutorials;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string Name { get; set; }

    [BindProperty]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [BindProperty]
    public DateTime EndDate { get; set; } = DateTime.Today;

    public async Task<IActionResult> OnGet()
    {
        // If ID is empty, this is a create action
        // otherwise this is an edit and we need to populate the current values
        if (Id.HasValue)
        {
            var entry = await _mediator.Send(new GetTutorialByIdQuery(GroupTutorialId.FromValue(Id.Value)));

            if (entry.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    entry.Error,
                    _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Index", values: new { area = "Staff" }));

                return Page();
            }

            Name = entry.Value.Name;
            StartDate = entry.Value.StartDate.ToDateTime(TimeOnly.MinValue);
            EndDate = entry.Value.EndDate.ToDateTime(TimeOnly.MinValue);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        if (Id.HasValue)
        {
            // Update existing entry

            var command = new EditGroupTutorialCommand(
                GroupTutorialId.FromValue(Id.Value),
                Name,
                DateOnly.FromDateTime(StartDate),
                DateOnly.FromDateTime(EndDate));

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    result.Error,
                    _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Upsert", values: new { area = "Staff", Id = Id.Value }));
            }
        }
        else
        {
            // Create new entry
            var command = new CreateGroupTutorialCommand(
                Name,
                DateOnly.FromDateTime(StartDate),
                DateOnly.FromDateTime(EndDate));

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure && result.GetType() == typeof(ValidationResult<GroupTutorialId>))
            {
                foreach (var error in ((ValidationResult<GroupTutorialId>)result).Errors)
                {
                    ModelState.AddModelError(error.Code, error.Message);
                }

                return Page();
            }
        }

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Index", new { area = "Staff" });
    }
}
