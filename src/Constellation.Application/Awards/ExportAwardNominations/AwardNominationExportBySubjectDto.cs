﻿namespace Constellation.Application.Awards.ExportAwardNominations;

using System.Collections.Generic;

public sealed record AwardNominationExportBySubjectDto(
    string Subject)
{
    public List<AwardNominationExportByStudentDto> Students { get; set; } = new();
}