namespace Constellation.Core.Models.EmergencyConsole;

using Constellation.Core.Models.EmergencyConsole.Identifiers;
using Constellation.Core.ValueObjects;

public sealed record QueuedMessage(
    EventId EventId,
    MessageId MessageId,
    AlertRecipient AlertRecipient);