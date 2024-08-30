namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Schools.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISchoolRepository
{
    void Insert(School school);
    Task<List<School>> GetAllActive(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all Partner Schools that have active students
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<School>> GetAllActiveWithStudents(CancellationToken cancellationToken = default);
    Task<List<School>> GetAllInactive(CancellationToken cancellationToken = default);
    Task<School?> GetById(string id, CancellationToken cancellationToken = default);
    Task<List<School>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all Partner Schools with their related student records
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<School>> GetWithCurrentStudents(CancellationToken cancellationToken = default);
    Task<List<School>> GetListFromIds(List<string> schoolCodes, CancellationToken cancellationToken = default);
    Task<School> ForEditAsync(string id);
    Task<bool> IsPartnerSchoolWithStudents(string code);
    Task<bool> AnyWithId(string id);
    IList<MapLayer> GetForMapping(IList<string> schoolCodes);
    Task<SchoolType> GetSchoolType(string schoolCode, CancellationToken cancellationToken = default);
}