namespace Constellation.Presentation.Schools.Areas;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Application.Schools.GetSchoolsForContact;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Schools.Pages.Shared.Components.SchoolSelector;
using System.Security.Claims;

public class BasePageModel : PageModel, IBaseModel
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceFactory;

    public BasePageModel(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceFactory = serviceFactory;

        if (httpContextAccessor.HttpContext is null)
            return;

        bool success = httpContextAccessor.HttpContext.Session.TryGetValue(nameof(CurrentSchoolCode), out byte[]? currentSchoolCode);

        if (success && currentSchoolCode.Length > 0)
            CurrentSchoolCode = System.Text.Encoding.Default.GetString(currentSchoolCode);
        else
            CurrentSchoolCode = SetDefaultSchool();
    }

    public string? CurrentSchoolCode { get; set; }

    public ModalContent? ModalContent { get; set; }

    public async Task<IActionResult> OnPostChangeSchool(SchoolSelectorViewModel viewModel)
    {
        CurrentSchoolCode = viewModel.NewSchoolCode;

        _httpContextAccessor.HttpContext.Session.SetString(nameof(BasePageModel.CurrentSchoolCode), viewModel.NewSchoolCode);

        return RedirectToPage();
    }

    public string SetDefaultSchool()
    {
        using IServiceScope scope = _serviceFactory.CreateScope();
        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        UserManager<AppUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        ClaimsPrincipal User = _httpContextAccessor.HttpContext.User;

        AppUser user = userManager.FindByNameAsync(User.Identity?.Name).Result;

        Result<List<SchoolResponse>> schoolsRequest = User.IsInRole(AuthRoles.Admin)
            ? mediator.Send(new GetSchoolsForContactQuery(null, true)).Result
            : mediator.Send(new GetSchoolsForContactQuery(user.SchoolContactId)).Result;

        if (schoolsRequest.IsFailure)
            return string.Empty;

        SchoolResponse school = schoolsRequest.Value.MinBy(school => school.SchoolCode);

        _httpContextAccessor.HttpContext.Session.SetString(nameof(BasePageModel.CurrentSchoolCode), school.SchoolCode);

        return school.SchoolCode;
    }
}
