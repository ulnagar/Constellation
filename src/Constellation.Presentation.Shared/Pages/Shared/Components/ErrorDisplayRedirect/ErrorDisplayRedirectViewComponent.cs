namespace Constellation.Presentation.Shared.Pages.Shared.Components.ErrorDisplayRedirect;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc;

public class ErrorDisplayRedirectViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ErrorDisplay error) => View(error);
}
