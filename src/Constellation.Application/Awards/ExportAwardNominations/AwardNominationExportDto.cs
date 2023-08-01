namespace Constellation.Application.Awards.ExportAwardNominations;

using System.Collections.Generic;

public sealed record AwardNominationExportDto(
    string SRN,
    string StudentFirstName,
    string StudentLastName,
    string StudentName,
    string Grade,
    string School,
    string Awards);
