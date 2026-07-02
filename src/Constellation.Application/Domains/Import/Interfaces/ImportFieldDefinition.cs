namespace Constellation.Application.Domains.Import.Interfaces;

public sealed record ImportFieldDefinition(
    string Key,
    string Label,
    bool Required,
    string? GroupLabel = null); // groups e.g. the 3 name columns together in the UI