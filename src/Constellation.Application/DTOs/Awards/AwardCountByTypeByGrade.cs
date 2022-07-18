namespace Constellation.Application.DTOs.Awards
{
    public class AwardCountByTypeByGrade
    {
        public string ReportPeriod { get; set; }
        public string Grade { get; set; }
        public string AwardType { get; set; }
        public int Count { get; set; }
    }
}
