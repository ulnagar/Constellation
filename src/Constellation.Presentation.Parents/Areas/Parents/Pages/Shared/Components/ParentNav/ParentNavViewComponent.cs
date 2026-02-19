namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Shared.Components.ParentNav;

using Application.Domains.Families.Queries.IsResidentialParent;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class ParentNavViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISender _mediator;

    public ParentNavViewComponent(
        ICurrentUserService currentUserService,
        ISender mediator)
    {
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
        
        Result<bool> isResidentialParent = await _mediator.Send(new IsResidentialParentQuery(_currentUserService.EmailAddress));

        viewModel.ShowConsent = (isResidentialParent.IsFailure || isResidentialParent.Value == false)
            ? false
            : true;
                
        return View("ParentNav", viewModel);
    }
}