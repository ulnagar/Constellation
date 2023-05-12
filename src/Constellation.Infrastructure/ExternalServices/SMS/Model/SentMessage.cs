namespace Constellation.Infrastructure.ExternalServices.SMS.Model;

internal class SentMessage
{
    public string id { get; set; }
    public string outgoing_id { get; set; }
    public string origin { get; set; }
    public string message { get; set; }
    public string dateTime { get; set; }
    public string status { get; set; }
    public string destination { get; set; }
}
