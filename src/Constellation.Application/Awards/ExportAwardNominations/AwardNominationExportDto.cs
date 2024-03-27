namespace Constellation.Application.Awards.ExportAwardNominations;

public sealed record AwardNominationExportDto(
    string SRN,
    string StudentFirstName,
    string StudentLastName,
    string StudentName,
    string Grade,
    string School,
    string Awards);
