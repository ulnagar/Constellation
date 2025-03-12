namespace Constellation.Core.Models.SchoolContacts.Repositories;

using Core.Enums;
using Enums;
using SchoolContacts;
using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISchoolContactRepository
{
    Task<List<SchoolContact>> GetAll(CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetAllWithRole(CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetAllWithoutRole(CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetPrincipalsForSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<SchoolContact> GetWithRolesByEmailAddress(string emailAddress, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetWithRolesBySchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetByGrade(Grade grade, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetBySchoolAndRole(string schoolCode, Position selectedRole, CancellationToken cancellationToken = default);
    Task<SchoolContact> GetById(SchoolContactId contactId, CancellationToken cancellationToken = default);
    Task<SchoolContact> GetByNameAndSchool(string name, string schoolCode, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetAllByRole(Position selectedRole, CancellationToken cancellationToken = default);
    void Insert(SchoolContact schoolContact);
}