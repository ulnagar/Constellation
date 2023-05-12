namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDoEDataSourcesGateway
{
    Task<List<CeseSchoolResponse>> GetSchoolsFromCESEMasterData();
    Task<List<DataCollectionsSchoolResponse>> GetSchoolsFromDataCollections();
}
