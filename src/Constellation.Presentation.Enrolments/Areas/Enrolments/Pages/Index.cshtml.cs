namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages;

using Application.Models.Auth;
using Constellation.Presentation.Enrolments.Areas.Enrolments.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = AuthPolicies.IsEnrolmentsPortalUser)]
public class IndexModel : BasePageModel
{
    public IndexModel()
    {
        
    }
    
    public async Task OnGet()
    {
    }
}