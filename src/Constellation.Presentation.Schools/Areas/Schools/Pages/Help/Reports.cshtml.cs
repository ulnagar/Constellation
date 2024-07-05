namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Help;

using Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class ReportsModel : BasePageModel
{
    public ReportsModel(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceFactory)
        : base(httpContextAccessor, serviceFactory)
    {
    }

    [ViewData] public string ActivePage => Models.ActivePage.Help;

    public void OnGet()
    {
    }
}