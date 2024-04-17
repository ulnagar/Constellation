namespace Constellation.Core.Models.WorkFlow;

using Identifiers;

public sealed class InterviewAttendee
{
    private InterviewAttendee() { }

    public InterviewAttendeeId Id { get; private set; } = new();
    public ActionId ActionId { get; private set; }
    public string Name { get; private set; }
    public string Notes { get; private set; }

    public static InterviewAttendee Create(
        ActionId actionId,
        string name,
        string notes)
    {
        InterviewAttendee attendee = new()
        {
            ActionId = actionId,
            Name = name,
            Notes = notes
        };

        return attendee;
    }
}