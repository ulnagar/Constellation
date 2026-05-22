namespace Constellation.Presentation.Schools.Areas;

using Application.Domains.Schools.Queries.GetSchoolsForContact;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Schools.Pages.Shared.Components.SchoolSelectorModal;
using System.ComponentModel;
using System.Security.Claims;

public class BasePageModel : PageModel, IBaseModel
{
    protected ISender Mediator =>
        HttpContext.RequestServices.GetRequiredService<ISender>();
    
    private IAuthorizationService AuthService =>
        HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
    
    private UserManager<AppUser> UserManager =>
        HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();

    public SchoolCode CurrentSchoolCode { get; set; } = SchoolCode.Empty;
    public ModalContent? ModalContent { get; set; }

    // 2026-05-22: Added to remove the SetDefaultSchool call from the constructor, allowing it to be async
    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        string? stringCode = HttpContext.Session.GetString(nameof(CurrentSchoolCode));

        if (!string.IsNullOrWhiteSpace(stringCode))
        {
            Result<SchoolCode> schoolCode = SchoolCode.TryFromValue(stringCode);

            CurrentSchoolCode = schoolCode.IsSuccess
                ? schoolCode.Value
                : await SetDefaultSchool();
        }
        else
            CurrentSchoolCode = await SetDefaultSchool();

        await next();
    }

    public async Task<IActionResult> OnPostChangeSchool(SchoolSelectorModalViewModel viewModel)
    {
        CurrentSchoolCode = viewModel.NewSchoolCode;

        HttpContext.Session.SetString(nameof(CurrentSchoolCode), viewModel.NewSchoolCode.ToString());

        return RedirectToPage();
    }

    private async Task<SchoolCode> SetDefaultSchool()
    {
        ClaimsPrincipal? httpContextUser = HttpContext.User;

        if (httpContextUser.Identity is null)
            return SchoolCode.Empty;

        AppUser? user = await UserManager.FindByNameAsync(httpContextUser.Identity.Name ?? string.Empty);

        AuthorizationResult isAdminTest = await AuthService.AuthorizeAsync(httpContextUser, AuthPolicies.IsSiteAdmin);

        AppUserLink? contactLink = user?.Links.FirstOrDefault(link => !link.IsDeleted && link.Type == LinkType.Contact);

        if (contactLink is null && !isAdminTest.Succeeded)
            return SchoolCode.Empty;

        SchoolContactId contactId = contactLink is not null
            ? SchoolContactId.FromValue(contactLink.LinkId)
            : SchoolContactId.Empty;

        GetSchoolsForContactQuery schoolListQuery = isAdminTest.Succeeded
            ? new(SchoolContactId.Empty, true)
            : new(contactId);
        
        Result<List<SchoolResponse>> schoolsRequest = await Mediator.Send(schoolListQuery);

        if (schoolsRequest.IsFailure || schoolsRequest.Value.Count == 0)
            return SchoolCode.Empty;

        SchoolResponse school = schoolsRequest.Value.MinBy(school => school.SchoolCode.ToString())!;

        HttpContext.Session.SetString(nameof(CurrentSchoolCode), school.SchoolCode.ToString());

        return school.SchoolCode;
    }
}
