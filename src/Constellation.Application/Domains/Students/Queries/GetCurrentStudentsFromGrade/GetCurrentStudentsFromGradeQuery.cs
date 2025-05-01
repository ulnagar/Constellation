namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsFromGrade;

using Abstractions.Messaging;
using Core.Enums;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentStudentsFromGradeQuery(
    Grade Grade)
    : IQuery<List<StudentResponse>>;