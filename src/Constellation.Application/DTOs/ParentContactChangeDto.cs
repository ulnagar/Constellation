namespace Constellation.Infrastructure.Jobs;

public sealed record ParentContactChangeDto(
    string Name,
    string OldEmail,
    string NewEmail,
    string StudentName,
    string Issue);