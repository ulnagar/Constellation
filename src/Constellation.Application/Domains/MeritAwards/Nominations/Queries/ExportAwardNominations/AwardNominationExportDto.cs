﻿namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.ExportAwardNominations;

using Core.Enums;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;

public sealed record AwardNominationExportDto(
    StudentReferenceNumber SRN,
    Name StudentName,
    Grade Grade,
    string School,
    string Award);