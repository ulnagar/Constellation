namespace Constellation.Presentation.Applicants.Areas.Applicants.Pages.Shared.Components.ApplicantNav;

using Constellation.Core.Models.StudentOnboarding.Identifiers;
using Core.Helpers;
using Microsoft.AspNetCore.Mvc;

public class ApplicantNavViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var rawId = RouteData.Values["ApplicationId"]?.ToString();

        // Parse into your strongly-typed ID
        if (!ApplicationId.TryParse(rawId, out var applicationId))
        {
            // Handle missing/invalid ID — maybe render an empty nav or throw
            return Content(string.Empty);
        }

        ApplicantNavViewModel model = new() { ApplicationId = applicationId };
        return View("ApplicantNav", model);
    } 
}