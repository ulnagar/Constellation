namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Shared.Components.SchoolSelectorModal;

using Constellation.Application.Domains.Schools.Queries.GetSchoolsForContact;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public sealed class SchoolSelectorModalViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly IAuthorizationService _authService;

    public SchoolSelectorModalViewComponent(
        ISender mediator,
        UserManager<AppUser> userManager,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _userManager = userManager;
        _authService = authService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string selectedSchoolCode)
    {
        SchoolSelectorModalViewModel viewModel = new();

        AppUser? user = await _userManager.FindByNameAsync(User.Identity?.Name);

        ClaimsPrincipal claimsPrincipal = new (User);

        AuthorizationResult isAdminTest = await _authService.AuthorizeAsync(claimsPrincipal, AuthPolicies.IsSiteAdmin);

        AppUserLink? contactLink = user?.Links.FirstOrDefault(link => !link.IsDeleted && link.Type == LinkType.Contact);

        if (contactLink is null && !isAdminTest.Succeeded)
            return Content(string.Empty);

        SchoolContactId contactId = contactLink is not null
            ? SchoolContactId.FromValue(contactLink.LinkId)
            : SchoolContactId.Empty;

        Result<List<SchoolResponse>> schoolsRequest = isAdminTest.Succeeded
            ? await _mediator.Send(new GetSchoolsForContactQuery(SchoolContactId.Empty, true))
            : await _mediator.Send(new GetSchoolsForContactQuery(contactId));

        if (schoolsRequest.IsFailure || schoolsRequest.Value.Count == 0)
            return Content(string.Empty);

        viewModel.ValidSchools = schoolsRequest.Value
            .OrderBy(entry => entry.Name)
            .ToList();
        
        viewModel.CurrentSchool = string.IsNullOrWhiteSpace(selectedSchoolCode)
            ? schoolsRequest.Value.MinBy(school => school.SchoolCode)
            : viewModel.ValidSchools.FirstOrDefault(entry => entry.SchoolCode.ToString() == selectedSchoolCode);

        if (viewModel.CurrentSchool is null)
            if (viewModel.ValidSchools.Count > 0)
                viewModel.CurrentSchool = schoolsRequest.Value.MinBy(school => school.SchoolCode);

        viewModel.SchoolsList = new SelectList(
            viewModel.ValidSchools,
            nameof(SchoolResponse.SchoolCode),
            nameof(SchoolResponse.Name));

        viewModel.NewSchoolCode = viewModel.CurrentSchool?.SchoolCode ?? SchoolCode.Empty;
        
        return View("SchoolSelectorModal", viewModel);
    }
}
