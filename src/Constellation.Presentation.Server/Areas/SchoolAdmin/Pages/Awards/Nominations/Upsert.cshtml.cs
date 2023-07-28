namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Awards.Nominations;

using Constellation.Application.Awards.CreateNominationPeriod;
using Constellation.Core.Enums;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;

    public UpsertModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData]
    public string ActivePage => "Nominations";

    [BindProperty]
    public DateOnly LockoutDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    public List<Grade> Grades { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (!Grades.Any())
            ModelState.AddModelError("Grades", "You must select at least one grade");

        if (!ModelState.IsValid) 
            return Page();

        Result request = await _mediator.Send(new CreateNominationPeriodCommand(LockoutDate, Grades), cancellationToken);

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("Index", new { area = "SchoolAdmin" });
    }
}
