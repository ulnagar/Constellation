namespace Constellation.Presentation.Schools.Areas;

using Application.Domains.Schools.Queries.GetSchoolsForContact;
using Application.Models.Identity.Enums;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Shared;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Schools.Pages.Shared.Components.SchoolSelectorModal;
using System.Security.Claims;

public class BasePageModel : PageModel, IBaseModel
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceFactory;
    private readonly IAuthorizationService _authService;

    public BasePageModel(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceFactory,
        IAuthorizationService authService)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceFactory = serviceFactory;
        _authService = authService;

        if (httpContextAccessor.HttpContext is null)
            return;

        bool success = httpContextAccessor.HttpContext.Session.TryGetValue(nameof(CurrentSchoolCode), out byte[]? currentSchoolCode);

        if (success && currentSchoolCode.Length > 0)
        {
            var stringCode = System.Text.Encoding.Default.GetString(currentSchoolCode);

            var schoolCode = SchoolCode.TryFromValue(stringCode);

            CurrentSchoolCode = schoolCode.IsSuccess
                ? schoolCode.Value
                : SetDefaultSchool();
        }
        else
            CurrentSchoolCode = SetDefaultSchool();
    }

    public SchoolCode CurrentSchoolCode { get; set; } = SchoolCode.Empty;

    public ModalContent? ModalContent { get; set; }

    public async Task<IActionResult> OnPostChangeSchool(SchoolSelectorModalViewModel viewModel)
    {
        CurrentSchoolCode = viewModel.NewSchoolCode;

        _httpContextAccessor.HttpContext?.Session.SetString(nameof(BasePageModel.CurrentSchoolCode), viewModel.NewSchoolCode.ToString());

        return RedirectToPage();
    }

    public SchoolCode SetDefaultSchool()
    {
        using IServiceScope scope = _serviceFactory.CreateScope();
        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        UserManager<AppUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        ClaimsPrincipal? httpContextUser = _httpContextAccessor.HttpContext?.User;

        if (httpContextUser is null)
            return SchoolCode.Empty;

        AppUser? user = userManager.FindByNameAsync(httpContextUser.Identity?.Name ?? string.Empty).Result;

        AuthorizationResult isAdminTest = _authService.AuthorizeAsync(httpContextUser, AuthPolicies.IsSiteAdmin).Result;

        AppUserLink? contactLink = user?.Links.FirstOrDefault(link => !link.IsDeleted && link.Type == LinkType.Contact);

        if (contactLink is null && !isAdminTest.Succeeded)
            return SchoolCode.Empty;

        SchoolContactId contactId = contactLink is not null
            ? SchoolContactId.FromValue(contactLink.LinkId)
            : SchoolContactId.Empty;

        Result<List<SchoolResponse>> schoolsRequest = isAdminTest.Succeeded
            ? mediator.Send(new GetSchoolsForContactQuery(SchoolContactId.Empty, true)).Result
            : mediator.Send(new GetSchoolsForContactQuery(contactId)).Result;

        if (schoolsRequest.IsFailure || schoolsRequest.Value.Count == 0)
            return SchoolCode.Empty;

        SchoolResponse school = schoolsRequest.Value.MinBy(school => school.SchoolCode.ToString())!;

        _httpContextAccessor.HttpContext?.Session.SetString(nameof(CurrentSchoolCode), school!.SchoolCode.ToString());

        return school.SchoolCode;
    }
}
