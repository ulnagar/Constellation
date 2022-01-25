using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ISchoolService
    {
        Task<ServiceOperationResult<School>> CreateSchool(SchoolDto schoolResource);
        Task<ServiceOperationResult<School>> UpdateSchool(string code, SchoolDto schoolResource);
    }
}
