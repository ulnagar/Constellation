#nullable enable
namespace Constellation.Presentation.Server.BaseModels;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class BasePageModel : PageModel, IBaseModel
{
    public ErrorDisplay? Error { get; set; }
}
