using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface ILinkShortenerGateway
    {
        Task<string> ShortenURL(string url);
    }
}
