namespace Constellation.Core.Models.WorkFlow;

using Identifiers;
using System;

public sealed class ActionNote
{
    private ActionNote() { }

    public ActionNoteId Id { get; private set; } = new();
    public ActionId ActionId { get; private set; }
    public string Note { get; private set; }
    public string SubmittedBy { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    internal static ActionNote Create(
        string message,
        string submittedBy,
        DateTime submittedAt)
    {
        ActionNote note = new()
        {
            Note = message, 
            SubmittedBy = submittedBy, 
            SubmittedAt = submittedAt
        };

        return note;
    }
}