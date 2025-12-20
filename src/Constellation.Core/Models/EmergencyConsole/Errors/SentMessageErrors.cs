namespace Constellation.Core.Models.EmergencyConsole.Errors;

using Identifiers;
using Shared;

public static class SentMessageErrors
{
    public static Func<EventId, Error> NotFound = id => new(
        "EmergencyConsole.SentMessage.NotFound",
        $"Could not find a message with the Id '{id}'");

    public static Error AddressBlank = new(
        "EmergencyConsole.SentMessage.AddressBlank",
        "Address must not be blank");

    public static Error NameBlank = new(
        "EmergencyConsole.SentMessage.NameBlank",
        "Name must not be blank");

    public static Error MessageBlank = new(
        "EmergencyConsole.SentMessage.MessageBlank",
        "Message must not be blank");
}