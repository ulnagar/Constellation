﻿namespace Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;

using Core.Models.Students.Identifiers;

public sealed record StudentSelectionResponse(
    StudentId StudentId,
    string FirstName,
    string LastName,
    string CurrentGrade)
{
    public string DisplayName => $"{FirstName} {LastName}";
}