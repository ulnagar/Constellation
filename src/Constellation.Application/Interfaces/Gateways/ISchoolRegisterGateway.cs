using Constellation.Application.DTOs.CSV;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface ISchoolRegisterGateway
    {
        Task GetSchoolPrincipals();
        Task<ICollection<CSVSchool>> GetSchoolList();
    }
}