namespace Constellation.Core.Models.WorkFlow;

using Identifiers;
using System;

public sealed class ActionNote
{
    private ActionNote() { }

    public ActionNoteId Id { get; private set; } = new();
    public ActionId ActionId { get; private set; } = ActionId.Empty;
    public string Note { get; private set; } = string.Empty;
    public string SubmittedBy { get; private set; } = string.Empty;
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