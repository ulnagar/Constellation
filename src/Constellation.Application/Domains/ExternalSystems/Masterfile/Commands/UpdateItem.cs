namespace Constellation.Application.Domains.ExternalSystems.Masterfile.Commands;
public sealed record UpdateItem(
    string Source,
    int Line,
    string LineDescription,
    string Column,
    string CurrentValue,
    string NewValue,
    string Action);
