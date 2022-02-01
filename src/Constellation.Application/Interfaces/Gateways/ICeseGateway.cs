using Constellation.Application.DTOs.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface ICeseGateway
    {
        Task<IList<JsonSchoolDto>> GetSchoolsFromMasterData();
    }
}
