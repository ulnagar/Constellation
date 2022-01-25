using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class School_GraphData
    {
        public string GraphDate { get; set; }
        public string GraphSiteName { get; set; }
        public string GraphIntlDate { get; set; }
        public ICollection<School_GraphDataPoint> GraphData { get; set; }

        public School_GraphData()
        {
            GraphData = new List<School_GraphDataPoint>();
        }
    }

    public class School_GraphDataPoint
    {
        public School_GraphDataPoint()
        {
            Networks = new List<School_GraphDataPointDetail>();
        }

        public string Time { get; set; }
        public bool Lesson { get; set; }
        public ICollection<School_GraphDataPointDetail> Networks { get; set; }
    }

    public class School_GraphDataPointDetail
    {
        public string Network { get; set; }
        public decimal Inbound { get; set; }
        public decimal Outbound { get; set; }
        public long? Connection { get; set; }
    }
}