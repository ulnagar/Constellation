namespace Constellation.Presentation.Parents.Areas.Parents.Models;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class BasePageModel : PageModel, IBaseModel
{
    public ModalContent? ModalContent { get; set; }
}
