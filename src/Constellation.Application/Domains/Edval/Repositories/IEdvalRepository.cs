namespace Constellation.Application.Domains.Edval.Repositories;

using Core.Models.Edval;
using System.Threading;
using System.Threading.Tasks;

public interface IEdvalRepository
{
    Task ClearClasses(CancellationToken cancellationToken = default);
    Task ClearClassMemberships(CancellationToken cancellationToken = default);
    Task ClearStudents(CancellationToken cancellationToken = default);
    Task ClearTeachers(CancellationToken cancellationToken = default);
    Task ClearTimetables(CancellationToken cancellationToken = default);

    void Insert(EdvalClass entity);
    void Insert(EdvalClassMembership entity);
    void Insert(EdvalStudent entity);
    void Insert(EdvalTeacher entity);
    void Insert(EdvalTimetable entity);

}
