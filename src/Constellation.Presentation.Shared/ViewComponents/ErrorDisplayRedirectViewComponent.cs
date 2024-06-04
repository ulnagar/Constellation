#nullable enable
namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc;

public class ErrorDisplayRedirectViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ErrorDisplay error) => View(error);
}
