namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Application.DTOs;
using Core.Models.Identifiers;

public interface INetworkStatisticsGateway
{
    Task<NetworkStatisticsSiteDto> GetSiteDetails(SchoolCode schoolCode);
    Task GetSiteUsage(NetworkStatisticsSiteDto site, int day = 0);
}