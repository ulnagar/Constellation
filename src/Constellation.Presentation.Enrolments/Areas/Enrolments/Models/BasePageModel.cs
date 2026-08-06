namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Models;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

public abstract class BasePageModel : PageModel
{
    public ModalContent? ModalContent { get; set; }
}
