namespace Constellation.Application.Awards.GetSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetSummaryForStudentQuery(
    string StudentId)
    : IQuery<StudentAwardSummaryResponse>;