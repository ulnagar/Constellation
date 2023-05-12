namespace Constellation.Infrastructure.ExternalServices.SMS.Model;

using Newtonsoft.Json;

internal class Response
{
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
