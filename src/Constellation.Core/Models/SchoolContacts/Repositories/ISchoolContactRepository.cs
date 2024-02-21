namespace Constellation.Core.Models.SchoolContacts.Repositories;

using Constellation.Core.Models.SchoolContacts;
using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISchoolContactRepository
{
    Task<List<SchoolContact>> GetPrincipalsForSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<SchoolContact> GetWithRolesByEmailAddress(string emailAddress, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetWithRolesBySchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetBySchoolAndRole(string schoolCode, string selectedRole, CancellationToken cancellationToken = default);
    Task<SchoolContact> GetById(SchoolContactId contactId, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetAllByRole(string selectedRole, CancellationToken cancellationToken = default);
    Task<List<string>> GetAvailableRoleList(CancellationToken cancellationToken = default);
    void Insert(SchoolContact schoolContact);
}