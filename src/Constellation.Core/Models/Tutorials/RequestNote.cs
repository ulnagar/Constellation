#nullable enable
namespace Constellation.Core.Models.Tutorials;

using Identifiers;
using System;

public sealed class RequestNote
{
    private RequestNote() { }

    private RequestNote(
        RequestId requestId,
        string message,
        string submittedBy,
        DateTime submittedAt)
    {
        Id = new();
        RequestId = requestId;
        Message = message;
        SubmittedBy = submittedBy;
        SubmittedAt = submittedAt;
    }

    public RequestNoteId Id { get; private set; }
    public RequestId RequestId { get; private set; }
    public string Message { get; private set; }
    public string SubmittedBy { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    
    internal static RequestNote Create(
        RequestId requestId,
        string message,
        string submittedBy,
        DateTime submittedAt)
    {
        RequestNote note = new(
            requestId,
            message, 
            submittedBy, 
            submittedAt);

        return note;
    }
}