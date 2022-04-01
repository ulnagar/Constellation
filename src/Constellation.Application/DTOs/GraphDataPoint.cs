using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class GraphData
    {
        public string Date { get; set; }
        public string SiteName { get; set; }
        public string IntlDate { get; set; }
        public ICollection<GraphDataPoint> Data { get; set; }

        public GraphData()
        {
            Data = new List<GraphDataPoint>();
        }
    }

    public class GraphDataPoint
    {
        public GraphDataPoint()
        {
            Networks = new List<GraphDataPointDetail>();
        }

        public string Time { get; set; }
        public bool Lesson { get; set; }
        public ICollection<GraphDataPointDetail> Networks { get; set; }
    }

    public class GraphDataPointDetail
    {
        public string Network { get; set; }
        public decimal Inbound { get; set; }
        public decimal Outbound { get; set; }
        public long? Connection { get; set; }
    }
}