using Constellation.Application.DTOs.Awards;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Reports.Models.Awards
{
    public class DashboardViewModel : BaseViewModel
    {
        public ICollection<AwardCountByTypeByGrade> ByTypeByGrade { get; set; }
        public ICollection<AwardCountByTypeByMonth> ByTypeByMonth { get; set; }
    }
}
