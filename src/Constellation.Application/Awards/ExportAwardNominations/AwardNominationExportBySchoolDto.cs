namespace Constellation.Application.Awards.ExportAwardNominations;

using System.Collections.Generic;

public sealed record AwardNominationExportBySchoolDto(
    string School,
    List<AwardNominationExportByStudentDto> Students);