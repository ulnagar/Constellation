namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestById;

using Core.Enums;
using Core.Models.Timetables;
using Core.Models.Tutorials.Enums;
using Core.Models.Tutorials.Identifiers;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record TutorialRequestDetailsResponse(
    RequestId RequestId,
    Name Student,
    Grade Grade,
    string School,
    TutorialType Type,
    string Subject,
    List<Period> Periods,
    string Justification,
    DateTime CreatedAt,
    RequestStatus Status,
    IReadOnlyList<TutorialRequestDetailsResponse.RequestNoteResponse> Notes)
{
    public sealed record RequestNoteResponse(
        RequestNoteId NoteId,
        string Message,
        string SubmittedBy,
        DateTime SubmittedAt);
}