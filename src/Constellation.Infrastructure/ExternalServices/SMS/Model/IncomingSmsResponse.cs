namespace Constellation.Infrastructure.ExternalServices.SMS.Model;

using Application.Domains.Messaging.Sms.Dtos;

internal sealed class IncomingSmsResponse
{
    public int Total { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
    public List<IncomingSmsEntry> Messages { get; set; } = [];
}