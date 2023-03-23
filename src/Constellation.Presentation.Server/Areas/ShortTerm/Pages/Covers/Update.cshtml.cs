namespace Constellation.Presentation.Server.Areas.ShortTerm.Pages.Covers;

using Constellation.Application.ClassCovers.GetCoverWithDetails;
using Constellation.Application.ClassCovers.UpdateCover;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditCovers)]
public class UpdateModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpdateModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    public string OfferingName { get; set; }
    [BindProperty]
    public DateOnly StartDate { get; set; }
    [BindProperty]
    public DateOnly EndDate { get; set; }
    public string TeacherSchool { get; set; }
    public string TeacherName { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await PreparePage(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCoverCommand(ClassCoverId.FromValue(Id), StartDate, EndDate), cancellationToken);

        if (result.IsFailure)
        {
            await PreparePage(cancellationToken);

            ModelState.AddModelError("", result.Error.Message);

            return Page();
        }

        return RedirectToPage("Index");
    }

    private async Task<bool> PreparePage(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        var coverResult = await _mediator.Send(new GetCoverWithDetailsQuery(ClassCoverId.FromValue(Id)), cancellationToken);

        if (coverResult.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = coverResult.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Covers/Index", values: new { area = "ShortTerm" })
            };

            return false;
        }

        OfferingName = coverResult.Value.OfferingName;
        StartDate = coverResult.Value.StartDate;
        EndDate = coverResult.Value.EndDate;
        TeacherName = coverResult.Value.UserName;
        TeacherSchool = coverResult.Value.UserSchool;

        return true;
    }
}
