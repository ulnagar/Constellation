namespace Constellation.Application.Students.GetFilteredStudentsForSelectionList;

using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;
using Core.Models.Students.Identifiers;

public sealed record StudentForSelectionList(
    StudentId StudentId,
    Name StudentName,
    Grade Grade);