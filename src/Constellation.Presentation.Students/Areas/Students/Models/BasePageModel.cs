namespace Constellation.Presentation.Students.Areas.Students.Models;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class BasePageModel : PageModel, IBaseModel
{
    public ModalContent? ModalContent { get; set; }
}
