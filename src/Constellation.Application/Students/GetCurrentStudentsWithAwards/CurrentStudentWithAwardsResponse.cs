namespace Constellation.Application.Students.GetCurrentStudentsWithAwards;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record CurrentStudentWithAwardsResponse(
    string StudentId,
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
