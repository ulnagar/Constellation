namespace Constellation.Application.Domains.MeritAwards.Awards.Models;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;

public sealed record AwardResponse(
    StudentAwardId AwardId,
    Name StudentName,
    Grade StudentGrade,
    string SchoolName,
    Name TeacherName,
    DateTime AwardedOn,
    string Type,
    bool HasCertificate);