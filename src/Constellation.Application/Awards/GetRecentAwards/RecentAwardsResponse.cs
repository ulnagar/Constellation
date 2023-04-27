namespace Constellation.Application.Awards.GetRecentAwards;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record RecentAwardResponse(
    StudentAwardId AwardId,
    string StudentId,
    string Name,
    string Grade,
    string School,
    string AwardType,
    DateTime AwardedOn);