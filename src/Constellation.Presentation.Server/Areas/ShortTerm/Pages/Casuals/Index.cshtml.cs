namespace Constellation.Presentation.Server.Areas.ShortTerm.Pages.Casuals;

using Constellation.Application.Casuals.DeleteCasual;
using Constellation.Application.Casuals.GetActiveCasuals;
using Constellation.Application.Casuals.GetAllCasuals;
using Constellation.Application.Casuals.GetInactiveCasuals;
using Constellation.Application.Casuals.Models;
using Constellation.Application.Casuals.RestoreCasual;
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

    public List<CasualsListResponse> Casuals = new();

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public async Task OnGet(CancellationToken cancellationToken)
    {
        var casualsRequest = Filter switch
        {
            FilterDto.All => await _mediator.Send(new GetAllCasualsQuery(), cancellationToken),
            FilterDto.Active => await _mediator.Send(new GetActiveCasualsQuery(), cancellationToken),
            FilterDto.Inactive => await _mediator.Send(new GetInactiveCasualsQuery(), cancellationToken)
        };

        if (casualsRequest is not null && casualsRequest.IsSuccess)
        {
            Casuals = casualsRequest.Value.OrderBy(casual => casual.LastName).ToList();
        }
    }

    public async Task<IActionResult> OnGetDelete(Guid Id, CancellationToken cancellationToken)
    {
        var authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditCasuals);

        if (authorised.Succeeded)
        {
            await _mediator.Send(new DeleteCasualCommand(CasualId.FromValue(Id)), cancellationToken);
        }

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnGetRestore(Guid Id, CancellationToken cancellationToken)
    {
        var authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditCasuals);

        if (authorised.Succeeded)
        {
            await _mediator.Send(new RestoreCasualCommand(CasualId.FromValue(Id)), cancellationToken);
        }

        return RedirectToPage("Index");
    }

    public enum FilterDto
    {
        All,
        Active,
        Inactive
    }
}
