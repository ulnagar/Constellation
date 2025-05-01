namespace Constellation.Application.Domains.Students.Queries.GetFilteredStudentsForSelectionList;

using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;
using Core.Models.Students.Identifiers;

public sealed record StudentForSelectionList(
    StudentId StudentId,
    Name StudentName,
    Grade Grade);