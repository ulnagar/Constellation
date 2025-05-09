namespace Constellation.Application.Domains.Edval.Repositories;

using Constellation.Core.Models.Edval.Enums;
using Constellation.Core.Primitives;
using Core.Models.Edval;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IEdvalRepository
{
    Task<int> CountDifferences(CancellationToken cancellationToken = default);
    Task<List<Difference>> GetDifferences(CancellationToken cancellationToken = default);
    Task<List<EdvalClass>> GetClasses(CancellationToken cancellationToken = default);
    Task<List<EdvalClassMembership>> GetClassMemberships(CancellationToken cancellationToken = default);
    Task<List<EdvalStudent>> GetStudents(CancellationToken cancellationToken = default);
    Task<List<EdvalTeacher>> GetTeachers(CancellationToken cancellationToken = default);
    Task<List<EdvalTimetable>> GetTimetables(CancellationToken cancellationToken = default);

    Task ClearClasses(CancellationToken cancellationToken = default);
    Task ClearClassMemberships(CancellationToken cancellationToken = default);
    Task ClearStudents(CancellationToken cancellationToken = default);
    Task ClearTeachers(CancellationToken cancellationToken = default);
    Task ClearTimetables(CancellationToken cancellationToken = default);
    Task ClearDifferences(EdvalDifferenceType type, CancellationToken cancellationToken = default);

    void Insert(EdvalClass entity);
    void Insert(EdvalClassMembership entity);
    void Insert(EdvalStudent entity);
    void Insert(EdvalTeacher entity);
    void Insert(EdvalTimetable entity);
    void Insert(Difference entity);
    void AddIntegrationEvent(IIntegrationEvent integrationEvent);

}
