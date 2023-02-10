namespace Constellation.Presentation.Server.Areas.ShortTerm.Models;

using Constellation.Application.ClassCovers.GetCoverWithDetails;
using Constellation.Presentation.Server.BaseModels;

public class Covers_DetailsViewModel : BaseViewModel
{
    public CoverWithDetailsResponse Cover { get; set; }
}