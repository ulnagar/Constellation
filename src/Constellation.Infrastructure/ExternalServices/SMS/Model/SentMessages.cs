namespace Constellation.Infrastructure.ExternalServices.SMS.Model;

internal class SentMessages : Response
{
    public SentMessage[] messages { get; set; }
}
