namespace Constellation.Presentation.Server.Areas.ShortTerm.Pages.Covers;

using Constellation.Application.ClassCovers.CancelCover;
using Constellation.Application.ClassCovers.GetAllCoversForCalendarYear;
using Constellation.Application.ClassCovers.GetAllCurrentAndFutureCovers;
using Constellation.Application.ClassCovers.GetFutureCovers;
using Constellation.Application.ClassCovers.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IAuthorizationService _authorizationService;

    public IndexModel(
        IMediator mediator,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
    }

    public List<CoversListResponse> Covers = new();

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Current;

    public async Task OnGet(CancellationToken cancellationToken)
    {
        await GetClasses(_mediator);

        var coverRequest = Filter switch
        {
            FilterDto.All => await _mediator.Send(new GetAllCoversForCalendarYearQuery(), cancellationToken),
            FilterDto.Current => await _mediator.Send(new GetAllCurrentAndFutureCoversQuery(), cancellationToken),
            FilterDto.Upcoming => await _mediator.Send(new GetFutureCoversQuery(), cancellationToken)
        };

        if (coverRequest is not null && coverRequest.IsSuccess)
        {
            Covers = coverRequest.Value.OrderBy(cover => cover.StartDate).ToList();
        }
    }

    public async Task<IActionResult> OnGetCancel(Guid Id, CancellationToken cancellationToken)
    {
        var authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditCovers);

        if (authorised.Succeeded)
        {
            await _mediator.Send(new CancelCoverCommand(ClassCoverId.FromValue(Id)), cancellationToken);
        }

        return RedirectToPage("Index");
    }

    public enum FilterDto
    {
        All,
        Current,
        Upcoming
    }
}
