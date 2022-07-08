namespace Constellation.Application.DTOs.Awards
{
    public class AwardCountByTypeByMonth
    {
        public string MonthName { get; set; }
        public string MonthSort { get; set; }
        public string AwardType { get; set; }
        public int Count { get; set; }
    }
}
