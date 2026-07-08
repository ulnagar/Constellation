namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using Core.Models.Identifiers;
using Domains.Schools.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISchoolRepository
{
    void Insert(School school);
    Task<List<School>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<School>> GetAllInactive(CancellationToken cancellationToken = default);
    Task<School?> GetById(SchoolCode schoolCode, CancellationToken cancellationToken = default);
    Task<School?> GetByName(string schoolName, CancellationToken cancellationToken = default);
    Task<List<School>> GetAll(CancellationToken cancellationToken = default);
    Task<List<School>> GetWithCurrentStudents(CancellationToken cancellationToken = default);
    Task<List<School>> GetListFromIds(List<SchoolCode> schoolCodes, CancellationToken cancellationToken = default);
    Task<bool> IsPartnerSchoolWithStudents(SchoolCode schoolCode, CancellationToken cancellationToken = default);
    Task<SchoolType> GetSchoolType(SchoolCode schoolCode, CancellationToken cancellationToken = default);
}