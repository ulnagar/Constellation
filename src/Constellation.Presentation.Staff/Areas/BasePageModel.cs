#nullable enable
namespace Constellation.Presentation.Staff.Areas;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class BasePageModel : PageModel, IBaseModel
{
    public ModalContent? ModalContent { get; set; }
}
