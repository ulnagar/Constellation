using System;
using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class NetworkStatisticsSiteDto
    {
        public NetworkStatisticsSiteDto()
        {
            Standards = new List<SiteStandard>();

            WANData = new List<PointOfTimeUsage>();
            NETData = new List<PointOfTimeUsage>();
        }

        public int Id { get; set; }
        public string RouterName { get; set; }
        public string SiteName { get; set; }
        public string SchoolCode { get; set; }
        public double? Bandwidth { get; set; }
        public long WANBandwidth { get; set; }
        public long INTBandwidth { get; set; }
        public ICollection<SiteStandard> Standards { get; set; }
        public string BandwidthDisplay { get; set; }
        public ICollection<PointOfTimeUsage> WANData { get; set; }
        public ICollection<PointOfTimeUsage> NETData { get; set; }

        public class SiteStandard
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class PointOfTimeUsage
        {
            public DateTime Time { get; set; }
            public decimal WANInbound { get; set; }
            public string WANInboundDisplay { get; set; }
            public decimal WANOutbound { get; set; }
            public string WANOutboundDisplay { get; set; }
            public decimal INTInbound { get; set; }
            public string INTInboundDisplay { get; set; }
            public decimal INTOutbound { get; set; }
            public string INTOutboundDisplay { get; set; }
        }
    }
}
