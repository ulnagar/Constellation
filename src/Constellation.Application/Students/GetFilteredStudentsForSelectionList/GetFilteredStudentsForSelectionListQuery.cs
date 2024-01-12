namespace Constellation.Application.Students.GetFilteredStudentsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetFilteredStudentsForSelectionListQuery(
    List<Grade> FromGrades,
    List<OfferingId> FromOffering,
    List<CourseId> FromCourse)
    : IQuery<List<StudentForSelectionList>>;