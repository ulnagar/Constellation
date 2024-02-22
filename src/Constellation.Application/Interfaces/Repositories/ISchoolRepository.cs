namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
public interface ISchoolRepository
{
    void Insert(School school);
    Task<List<School>> GetAllActive(CancellationToken cancellationToken = default);
    Task<School?> GetById(string id, CancellationToken cancellationToken = default);
    Task<List<School>> GetAll(CancellationToken cancellationToken = default);
    Task<List<School>> GetWithCurrentStudents(CancellationToken cancellationToken = default);
    Task<List<School>> GetListFromIds(List<string> schoolCodes, CancellationToken cancellationToken = default);
    Task<ICollection<School>> ForSelectionAsync();
    Task<ICollection<School>> ForListAsync(Expression<Func<School, bool>> predicate);
    Task<School> ForEditAsync(string id);
    Task<School> ForDetailDisplayAsync(string id);
    Task<bool> IsPartnerSchoolWithStudents(string code);
    Task<bool> AnyWithId(string id);
    IList<MapLayer> GetForMapping(IList<string> schoolCodes);
}