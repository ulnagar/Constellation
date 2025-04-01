namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Shared.Components.ParentNav;

using Application.Interfaces.Configuration;
using Constellation.Application.Parents.IsResidentialParent;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

public class ParentNavViewComponent : ViewComponent
{
    private readonly IOptions<ParentPortalConfiguration> _configuration;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISender _mediator;

    public ParentNavViewComponent(
        IOptions<ParentPortalConfiguration> configuration,
        ICurrentUserService currentUserService,
        ISender mediator)
    {
        _configuration = configuration;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(string activePage)
    {
        ParentNavViewModel viewModel = new()
        {
            ActivePage = activePage,
            ShowConsent = false
        };

        if (_configuration.Value.ShowConsent)
        {
            Result<bool> isResidentialParent = await _mediator.Send(new IsResidentialParentQuery(_currentUserService.EmailAddress));

            viewModel.ShowConsent = (isResidentialParent.IsFailure || isResidentialParent.Value == false)
                ? false
                : true;
        }
                
        return View("ParentNav", viewModel);
    }
}