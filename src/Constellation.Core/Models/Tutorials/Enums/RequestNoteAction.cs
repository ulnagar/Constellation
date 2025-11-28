namespace Constellation.Core.Models.Tutorials.Enums;

using Common;
using System.Collections.Generic;

public sealed class RequestNoteAction : StringEnumeration<RequestNoteAction>
{
    public static readonly RequestNoteAction Approved = new("Approved", "Approved");
    public static readonly RequestNoteAction Scheduled = new("Scheduled", "Scheduled");
    public static readonly RequestNoteAction Rejected = new("Rejected", "Rejected");
    public static readonly RequestNoteAction Note = new("Note", "Note");

    private RequestNoteAction(string value, string name)
        : base(value, name) { }

    public static IEnumerable<RequestNoteAction> GetOptions => GetEnumerable;
}