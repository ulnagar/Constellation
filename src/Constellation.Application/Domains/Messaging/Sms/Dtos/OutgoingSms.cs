namespace Constellation.Application.Domains.Messaging.Sms.Dtos;

public sealed class OutgoingSms
{
    public string origin { get; set; }
    public string message { get; set; }
    public List<string> destinations { get; set; } = [];
    public string notifyUrl { get; set; }
}