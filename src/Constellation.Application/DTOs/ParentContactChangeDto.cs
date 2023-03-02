namespace Constellation.Infrastructure.Jobs;

public sealed record ParentContactChangeDto(
    string Name,
    string OldEmail,
    string NewEmail,
    string StudentFirstName,
    string StudentLastName,
    string StudentGrade,
    string Issue);