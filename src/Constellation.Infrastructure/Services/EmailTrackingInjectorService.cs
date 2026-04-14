namespace Constellation.Infrastructure.Services;

using Application.Domains.Messaging.Tracking.Models;
using Application.Interfaces.Services;
using Core.Models.Messaging.Email.Identifiers;
using HtmlAgilityPack;

internal sealed class EmailTrackingInjectorService : IEmailTrackingInjectorService
{
    private readonly string _baseUrl = "https://acos.aurora.nsw.edu.au";

    public InjectionResult InjectTracking(string bodyHtml, EmailId emailId)
    {
        (string html, IReadOnlyList<string> links) = InjectLinkTracking(bodyHtml, emailId);
        string withPixel = InjectTrackingPixel(html, emailId);

        return new InjectionResult(withPixel, links);
    }

    private (string Html, IReadOnlyList<string> DiscoveredLinks) InjectLinkTracking(string bodyHtml, EmailId emailId)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(bodyHtml);

        HtmlNodeCollection? links = doc.DocumentNode.SelectNodes("//a[@href]");
        List<string> discoveredLinks = [];

        if (links is null)
            return (bodyHtml, discoveredLinks);

        foreach (HtmlNode link in links)
        {
            string href = link.GetAttributeValue("href", string.Empty);

            if (string.IsNullOrEmpty(href)
                || href.StartsWith("#")
                || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("tel:", StringComparison.InvariantCultureIgnoreCase))
                continue;

            if (!discoveredLinks.Contains(href))
                discoveredLinks.Add(href);

            string encodedDestination = Uri.EscapeDataString(href);
            string trackingUrl = $"{_baseUrl}/track/click/{emailId}?url={encodedDestination}";

            link.SetAttributeValue("href", trackingUrl);
        }

        return (doc.DocumentNode.OuterHtml, discoveredLinks);
    }

    private string InjectTrackingPixel(string bodyHtml, EmailId emailId)
    {
        string pixel = BuildPixel(emailId);

        return bodyHtml.Contains("</body>", StringComparison.OrdinalIgnoreCase)
            ? bodyHtml.Replace("</body>", pixel + "</body>", StringComparison.OrdinalIgnoreCase)
            : bodyHtml + pixel;
    }

    private string BuildPixel(EmailId emailId) =>
        $"""
         <img src="{_baseUrl}/track/open/{emailId.ToString()}" 
              width="1" height="1" 
              style="display:none;border:0;outline:0;text-decoration:none" 
              alt="" />
         """;
}