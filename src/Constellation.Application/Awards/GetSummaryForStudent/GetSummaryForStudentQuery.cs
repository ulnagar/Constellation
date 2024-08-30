namespace Constellation.Application.Awards.GetSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record GetSummaryForStudentQuery(
    StudentId StudentId)
    : IQuery<StudentAwardSummaryResponse>;