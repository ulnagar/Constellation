using Constellation.Application.Features.Awards.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Reports.Models.Awards
{
    public class RecentAwardsListViewModel : BaseViewModel
    {
        public ICollection<AwardWithStudentName> Awards { get; set; }
    }
}
