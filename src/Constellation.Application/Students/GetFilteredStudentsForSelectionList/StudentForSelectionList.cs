namespace Constellation.Application.Students.GetFilteredStudentsForSelectionList;

using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;

public sealed record StudentForSelectionList(
    string StudentId,
    Name StudentName,
    Grade Grade);