namespace Constellation.Core.Models.Messaging.Email;

using Identifiers;

public sealed class EmailLink
{
    public EmailId EmailId { get; set; }
    public required string DestinationUrl { get; set; }

    public int ClickCount { get; set; } = 0;
    public DateTimeOffset? FirstClickedAt { get; set; }
    public DateTimeOffset? LastClickedAt { get; set; }
}