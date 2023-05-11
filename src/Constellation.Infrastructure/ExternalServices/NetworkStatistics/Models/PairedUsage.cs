namespace Constellation.Infrastructure.ExternalServices.NetworkStatistics.Models;

internal class PairedUsage
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
