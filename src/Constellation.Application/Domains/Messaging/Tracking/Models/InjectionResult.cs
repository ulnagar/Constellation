namespace Constellation.Application.Domains.Messaging.Tracking.Models;

public record InjectionResult(string Html, IReadOnlyList<string> DiscoveredLinks);