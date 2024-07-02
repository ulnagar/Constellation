namespace Constellation.Presentation.Shared.Pages.Shared.Components.ModalContentDisplay;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc;

public class ModalContentDisplayViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ModalContent content) => View(content);
}
