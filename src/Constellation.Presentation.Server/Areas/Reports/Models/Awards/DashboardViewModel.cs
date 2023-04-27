namespace Constellation.Presentation.Server.Areas.Reports.Models.Awards;

using Constellation.Application.Awards.GetAwardCountsByTypeByGrade;
using Constellation.Application.Awards.GetAwardCountsByTypeByMonth;
using Constellation.Presentation.Server.BaseModels;

public class DashboardViewModel : BaseViewModel
{
    public List<AwardCountByTypeByGradeResponse> ByTypeByGrade { get; set; }
    public List<AwardCountByTypeByMonthResponse> ByTypeByMonth { get; set; }
}
