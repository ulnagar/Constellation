namespace Constellation.Presentation.Server.Areas.Reports.Models.Awards;

using Constellation.Application.Awards.GetRecentAwards;
using Constellation.Application.Awards.Models;
using Constellation.Presentation.Server.BaseModels;

public class RecentAwardsListViewModel : BaseViewModel
{
    public List<AwardResponse> Awards { get; set; }
}
