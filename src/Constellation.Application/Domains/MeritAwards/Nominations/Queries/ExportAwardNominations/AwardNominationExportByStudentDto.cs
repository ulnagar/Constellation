namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.ExportAwardNominations;

using Core.Enums;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record AwardNominationExportByStudentDto(
    StudentReferenceNumber SRN,
    Name StudentName,
    Grade Grade,
    string School,
    List<string> Awards);