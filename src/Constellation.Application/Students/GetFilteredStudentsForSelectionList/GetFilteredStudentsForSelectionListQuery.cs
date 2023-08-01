namespace Constellation.Application.Students.GetFilteredStudentsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed record GetFilteredStudentsForSelectionListQuery(
    List<Grade> FromGrades,
    List<int> FromOffering,
    List<int> FromCourse)
    : IQuery<List<StudentForSelectionList>>;