namespace Constellation.Infrastructure.ExternalServices.SMS.Model;

internal class CreditBalance : Response
{
    public double balance { get; set; }
    public string currency { get; set; }
}
