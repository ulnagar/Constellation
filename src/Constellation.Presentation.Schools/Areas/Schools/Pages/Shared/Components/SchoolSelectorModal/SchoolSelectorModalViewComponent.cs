namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Shared.Components.SchoolSelector;

using Application.Domains.Schools.Queries.GetSchoolsForContact;
using Application.Models.Auth;
using Application.Models.Identity;
using Areas;
using Core.Models.SchoolContacts.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class SchoolSelectorModalViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    private readonly UserManager<AppUser> _userManager;

    public SchoolSelectorModalViewComponent(
        ISender mediator,
        UserManager<AppUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync(string selectedSchoolCode)
    {
        SchoolSelectorModalViewModel viewModel = new();

        AppUser user = await _userManager.FindByNameAsync(User.Identity?.Name);

        Result<List<SchoolResponse>> schoolsRequest = User.IsInRole(AuthRoles.Admin)
            ? await _mediator.Send(new GetSchoolsForContactQuery(SchoolContactId.Empty, true))
            : await _mediator.Send(new GetSchoolsForContactQuery(user.SchoolContactId));

        if (schoolsRequest.IsFailure || schoolsRequest.Value.Count == 0)
            return Content(string.Empty);

        viewModel.ValidSchools = schoolsRequest.Value
            .OrderBy(entry => entry.Name)
            .ToList();
        
        viewModel.CurrentSchool = string.IsNullOrWhiteSpace(selectedSchoolCode)
            ? schoolsRequest.Value.MinBy(school => school.SchoolCode)
            : viewModel.ValidSchools.FirstOrDefault(entry => entry.SchoolCode == selectedSchoolCode);

        if (viewModel.CurrentSchool is null)
            if (viewModel.ValidSchools.Count > 0)
                viewModel.CurrentSchool = schoolsRequest.Value.MinBy(school => school.SchoolCode);

        viewModel.SchoolsList = new SelectList(
            viewModel.ValidSchools, 
            nameof(SchoolResponse.SchoolCode),
            nameof(SchoolResponse.Name),
            viewModel.CurrentSchool?.SchoolCode);
        
        return View("SchoolSelectorModal", viewModel);
    }
}
