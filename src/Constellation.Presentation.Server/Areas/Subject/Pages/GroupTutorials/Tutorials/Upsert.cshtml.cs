namespace Constellation.Presentation.Server.Areas.Subject.Pages.GroupTutorials.Tutorials;

using Constellation.Application.GroupTutorials.CreateGroupTutorial;
using Constellation.Application.GroupTutorials.EditGroupTutorial;
using Constellation.Application.GroupTutorials.GetTutorialById;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanEditGroupTutorials)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(IMediator mediator, LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

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
        await GetClasses(_mediator);

        // If ID is empty, this is a create action
        // otherwise this is an edit and we need to populate the current values
        if (Id.HasValue)
        {
            var entry = await _mediator.Send(new GetTutorialByIdQuery(Id.Value));

            if (entry.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = entry.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/GroupTutorials/Tutorials/Index", values: new { area = "Subject" })
                };

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
                Id.Value,
                Name,
                DateOnly.FromDateTime(StartDate),
                DateOnly.FromDateTime(EndDate));

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/GroupTutorials/Tutorials/Upsert", values: new { area = "Subject", Id = Id.Value })
                };
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

            if (result.IsFailure && result.GetType() == typeof(ValidationResult<Guid>))
            {
                foreach (var error in ((ValidationResult<Guid>)result).Errors)
                {
                    ModelState.AddModelError(error.Code, error.Message);
                }

                return Page();
            }
        }

        return RedirectToPage("Index");
    }
}
