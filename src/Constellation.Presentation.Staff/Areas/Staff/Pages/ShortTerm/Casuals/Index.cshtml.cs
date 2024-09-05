namespace Constellation.Presentation.Staff.Areas.Staff.Pages.ShortTerm.Casuals;

using Application.Common.PresentationModels;
using Constellation.Application.Casuals.DeleteCasual;
using Constellation.Application.Casuals.GetActiveCasuals;
using Constellation.Application.Casuals.GetAllCasuals;
using Constellation.Application.Casuals.GetInactiveCasuals;
using Constellation.Application.Casuals.Models;
using Constellation.Application.Casuals.RestoreCasual;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.ShortTerm_Casuals_Index;
    [ViewData] public string PageTitle => "Casual Teacher List";

    public List<CasualsListResponse> Casuals = new();

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public async Task OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnGetDelete(
        CasualId id, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditCasuals);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            await PreparePage(cancellationToken);

            return Page();
        }

        DeleteCasualCommand command = new(id);

        _logger
            .ForContext(nameof(DeleteCasualCommand), command, true)
            .Information("Requested to delete Casual Teacher by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to delete Casual Teacher by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            await PreparePage(cancellationToken);

            return Page();
        }
        
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRestore(
        CasualId id, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditCasuals);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            await PreparePage(cancellationToken);

            return Page();
        }

        RestoreCasualCommand command = new(id);

        _logger
            .ForContext(nameof(RestoreCasualCommand), command, true)
            .Information("Requested to reinstate Casual Teacher by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reinstate Casual Teacher by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            await PreparePage(cancellationToken);

            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve list of Casual Teachers by user {User}", _currentUserService.UserName);

        Result<List<CasualsListResponse>> casualsRequest = Filter switch
        {
            FilterDto.All => await _mediator.Send(new GetAllCasualsQuery(), cancellationToken),
            FilterDto.Active => await _mediator.Send(new GetActiveCasualsQuery(), cancellationToken),
            FilterDto.Inactive => await _mediator.Send(new GetInactiveCasualsQuery(), cancellationToken),
            _ => Result.Success(new List<CasualsListResponse>())
        };

        if (casualsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), casualsRequest.Error, true)
                .Warning("Failed to retrieve list of Casual Teachers by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(casualsRequest.Error);

            return;
        }

        Casuals = casualsRequest.Value.OrderBy(casual => casual.LastName).ToList();
    }

    public enum FilterDto
    {
        All,
        Active,
        Inactive
    }
}
