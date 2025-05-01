namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsWithAwards;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using System;
using System.Collections.Generic;

public sealed record CurrentStudentWithAwardsResponse(
    StudentId StudentId,
    string FirstName,
    string LastName,
    string DisplayName,
    string SchoolCode,
    string SchoolName,
    Grade Grade,
    List<CurrentStudentWithAwardsResponse.RegisteredAward> Awards)
{
    public sealed record RegisteredAward(
        StudentAwardId AwardId,
        string Category,
        string Type,
        DateTime AwardedOn);
}
