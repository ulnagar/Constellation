namespace Constellation.Infrastructure.ExternalServices.LinkShortener.Models;

internal class DynamicLink
{
    public DynamicLinkInfo dynamicLinkInfo { get; set; }

    public DynamicLinkSuffix suffix { get; set; }

    public DynamicLink()
    {
        dynamicLinkInfo = new DynamicLinkInfo();
        suffix = new DynamicLinkSuffix();
    }
}