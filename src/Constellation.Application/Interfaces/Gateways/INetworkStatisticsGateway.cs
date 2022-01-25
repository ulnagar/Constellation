using Constellation.Application.DTOs;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface INetworkStatisticsGateway
    {
        Task<NetworkStatisticsSiteDto> GetSiteDetails(string schoolCode);
        Task GetSiteUsage(NetworkStatisticsSiteDto site, int day = 0);
    }
}
