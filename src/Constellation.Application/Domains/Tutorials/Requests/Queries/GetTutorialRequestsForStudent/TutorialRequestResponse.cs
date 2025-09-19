namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestsForStudent;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Timetables;
using Core.Models.Tutorials.Enums;
using Core.Models.Tutorials.Identifiers;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record TutorialRequestResponse(
    RequestId Id,
    StudentId StudentId,
    Name Student,
    Grade Grade,
    string School,
    TutorialType Type,
    string Course,
    List<Period> Periods,
    string Justification,
    RequestStatus Status,
    IReadOnlyList<TutorialRequestResponse.RequestNoteResponse> Notes)
{
    public sealed record RequestNoteResponse(
        RequestNoteId NoteId,
        string Message,
        string SubmittedBy,
        DateTime SubmittedAt);
}
