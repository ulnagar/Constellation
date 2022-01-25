using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ISchoolContactService
    {
        Task<ServiceOperationResult<SchoolContact>> CreateContact(SchoolContactDto schoolContactResource);
        Task<ServiceOperationResult<SchoolContact>> UpdateContact(SchoolContactDto schoolContactResource);

        Task UndeleteContact(int id);
        Task RemoveContact(int? id);

        Task<ServiceOperationResult<SchoolContactRole>> CreateRole(SchoolContactRoleDto schoolContactRoleResource);
        Task<ServiceOperationResult<SchoolContactRole>> UpdateRole(SchoolContactRoleDto schoolContactRoleResource);
        Task RemoveRole(int id);
    }
}
