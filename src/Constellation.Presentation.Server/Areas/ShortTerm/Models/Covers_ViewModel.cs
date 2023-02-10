namespace Constellation.Presentation.Server.Areas.ShortTerm.Models;

using Constellation.Application.ClassCovers.Models;
using Constellation.Presentation.Server.BaseModels;

public class Covers_ViewModel : BaseViewModel
{
    public List<CoversListResponse> Covers { get; set; } = new();
}