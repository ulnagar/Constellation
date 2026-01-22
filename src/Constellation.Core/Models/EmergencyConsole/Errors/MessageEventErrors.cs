namespace Constellation.Core.Models.EmergencyConsole.Errors;

using Identifiers;
using Shared;

public static class MessageEventErrors
{
    public static Func<EventId, Error> NotFound = id => new(
        "EmergencyConsole.MessageEvent.NotFound",
        $"Could not find a message with the Id '{id}'");

    public static Error AddressBlank = new(
        "EmergencyConsole.MessageEvent.AddressBlank",
        "Address must not be blank");

    public static Error NameBlank = new(
        "EmergencyConsole.MessageEvent.NameBlank",
        "Name must not be blank");

    public static Error MessageBlank = new(
        "EmergencyConsole.MessageEvent.MessageBlank",
        "Message must not be blank");
}