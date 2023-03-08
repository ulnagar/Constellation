namespace Constellation.Application.ExternalDataConsistency;
public sealed record UpdateItem(
    string Source,
    int Line,
    string Column,
    string CurrentValue,
    string NewValue);
