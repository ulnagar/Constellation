namespace Constellation.Presentation.Server.Areas.Reports.Models.Awards;

using Constellation.Application.Awards.GetRecentAwards;
using Constellation.Presentation.Server.BaseModels;

public class RecentAwardsListViewModel : BaseViewModel
{
    public List<RecentAwardResponse> Awards { get; set; }
}
