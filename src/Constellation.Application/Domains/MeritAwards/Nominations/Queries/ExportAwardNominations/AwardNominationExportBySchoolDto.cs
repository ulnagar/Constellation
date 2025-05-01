namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.ExportAwardNominations;

using System.Collections.Generic;

public sealed record AwardNominationExportBySchoolDto(
    string School,
    List<AwardNominationExportByStudentDto> Students);