namespace Constellation.Infrastructure.Services;

public record InjectionResult(string Html, IReadOnlyList<string> DiscoveredLinks);