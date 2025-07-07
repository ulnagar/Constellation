namespace Constellation.Presentation.Staff.Areas.Staff.Pages.ShortTerm.Covers;

using Application.Common.PresentationModels;
using Application.Domains.ClassCovers.Commands.CancelCover;
using Application.Domains.ClassCovers.Models;
using Application.Domains.ClassCovers.Queries.GetAllCoversForCalendarYear;
using Application.Domains.ClassCovers.Queries.GetAllCurrentAndFutureCovers;
using Application.Domains.ClassCovers.Queries.GetFutureCovers;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
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
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.ShortTerm_Covers_Index;
    [ViewData] public string PageTitle => "Class Cover List";

    public List<CoversListResponse> Covers = new();

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Current;

    public async Task OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);
    
    public async Task<IActionResult> OnGetCancel(
        ClassCoverId id, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditCovers);

        if (!authorised.Succeeded)
        {
            ModalContent = ErrorDisplay.Create(DomainErrors.Auth.NotAuthorised);

            await PreparePage(cancellationToken);

            return Page();
        }

        CancelCoverCommand command = new(id);

        _logger
            .ForContext(nameof(CancelCoverCommand), command, true)
            .Information("Requested to cancel Class Cover by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to cancel Class Cover by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage(cancellationToken);

            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve list of Class Covers by user {User}", _currentUserService.UserName);

        Result<List<CoversListResponse>> coverRequest = Filter switch
        {
            FilterDto.All => await _mediator.Send(new GetAllCoversForCalendarYearQuery(), cancellationToken),
            FilterDto.Current => await _mediator.Send(new GetAllCurrentAndFutureCoversQuery(), cancellationToken),
            FilterDto.Upcoming => await _mediator.Send(new GetFutureCoversQuery(), cancellationToken),
            _ => Result.Success(new List<CoversListResponse>())
        };

        if (coverRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), coverRequest.Error, true)
                .Warning("Failed to retrieve list of Class Covers by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(coverRequest.Error);

            return;
        }

        Covers = coverRequest.Value.OrderBy(cover => cover.StartDate).ToList();
    }

    public enum FilterDto
    {
        All,
        Current,
        Upcoming
    }
}
