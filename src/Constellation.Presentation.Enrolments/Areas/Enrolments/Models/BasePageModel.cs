namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Models;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

public abstract class BasePageModel : PageModel
{
    public ModalContent? ModalContent { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (RouteData.Values.TryGetValue("offerToken", out var value)
            && value is string tokenString
            && Guid.TryParse(tokenString, out _))
        {
            HttpContext.Items["OfferToken"] = tokenString;
        }

        base.OnPageHandlerExecuting(context);
    }
}
