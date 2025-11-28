#nullable enable
namespace Constellation.Core.Models.Tutorials;

using Enums;
using Identifiers;
using System;

public sealed class RequestNote
{
    private RequestNote() { }

    private RequestNote(
        RequestId requestId,
        string message,
        RequestNoteAction action,
        string submittedBy,
        DateTime submittedAt)
    {
        Id = new();
        RequestId = requestId;
        Message = message;
        Action = action;
        SubmittedBy = submittedBy;
        SubmittedAt = submittedAt;
    }

    public RequestNoteId Id { get; private set; }
    public RequestId RequestId { get; private set; }
    public string Message { get; private set; }
    public RequestNoteAction Action { get; private set; }
    public string SubmittedBy { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    
    internal static RequestNote Create(
        RequestId requestId,
        string message,
        RequestNoteAction action,
        string submittedBy,
        DateTime submittedAt)
    {
        RequestNote note = new(
            requestId,
            message, 
            action,
            submittedBy, 
            submittedAt);

        return note;
    }
}