namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using Domains.Schools.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISchoolRepository
{
    void Insert(School school);
    Task<List<School>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<School>> GetAllInactive(CancellationToken cancellationToken = default);
    Task<School?> GetById(string id, CancellationToken cancellationToken = default);
    Task<List<School>> GetAll(CancellationToken cancellationToken = default);
    Task<List<School>> GetWithCurrentStudents(CancellationToken cancellationToken = default);
    Task<List<School>> GetListFromIds(List<string> schoolCodes, CancellationToken cancellationToken = default);
    Task<bool> IsPartnerSchoolWithStudents(string code, CancellationToken cancellationToken = default);
    Task<SchoolType> GetSchoolType(string schoolCode, CancellationToken cancellationToken = default);
}