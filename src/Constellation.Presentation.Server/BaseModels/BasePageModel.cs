#nullable enable
namespace Constellation.Presentation.Server.BaseModels;

using Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class BasePageModel : PageModel, IBaseModel
{
    public ModalContent? ModalContent { get; set; }
}
