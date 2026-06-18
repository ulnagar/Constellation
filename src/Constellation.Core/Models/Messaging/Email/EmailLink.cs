namespace Constellation.Core.Models.Messaging.Email;

using Identifiers;
using System.Security.Cryptography;
using System.Text;

public sealed class EmailLink
{
    private EmailLink() { }

    private EmailLink(
        EmailId emailId,
        string destinationUrl)
    {
        EmailId = emailId;
        DestinationUrl = destinationUrl;
        UrlHash = SHA256.HashData(Encoding.Unicode.GetBytes(destinationUrl));
    }

    public EmailId EmailId { get; private set; }
    public byte[] UrlHash { get; private set; }
    public string DestinationUrl { get; private set; }

    public int ClickCount { get; set; } = 0;
    public DateTimeOffset? FirstClickedAt { get; set; }
    public DateTimeOffset? LastClickedAt { get; set; }

    public static EmailLink Create(
        EmailId emailId,
        string destinationUrl) =>
        new(emailId, destinationUrl);
}