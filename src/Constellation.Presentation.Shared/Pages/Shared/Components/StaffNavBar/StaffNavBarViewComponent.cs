namespace Constellation.Presentation.Shared.Pages.Shared.Components.StaffNavBar;

using Application.Models.Auth;
using Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForTeacher;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public sealed class StaffNavBarViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authService;

    public StaffNavBarViewComponent(
        ISender mediator,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _authService = authService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        StaffNavBarViewModel viewModel = new();

        string username = User.Identity?.Name;

        if (username is null)
            return View("Default", viewModel);

        AuthorizationResult parentPortal = await _authService.AuthorizeAsync(UserClaimsPrincipal, AuthPolicies.IsParent);
        AuthorizationResult schoolPortal = await _authService.AuthorizeAsync(UserClaimsPrincipal, AuthPolicies.IsSchoolContact);

        viewModel.CanAccessParentPortal = parentPortal.Succeeded;
        viewModel.CanAccessSchoolPortal = schoolPortal.Succeeded;

        Result<List<TeacherOfferingResponse>> query =
            await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(StaffId.Empty, username));

        if (query.IsFailure)
            return View("Default", viewModel);

        viewModel.Classes = query.Value;

        return View("Default", viewModel);
    }
}