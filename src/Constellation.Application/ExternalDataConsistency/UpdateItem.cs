namespace Constellation.Application.ExternalDataConsistency;
public sealed record UpdateItem(
    string Source,
    int Line,
    string LineDescription,
    string Column,
    string CurrentValue,
    string NewValue,
    string Action);
