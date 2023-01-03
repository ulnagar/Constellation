namespace Constellation.Core.Abstractions;

using Constellation.Core.Models;
using System.Threading;
using System.Threading.Tasks;

public interface IFacultyRepository
{
    Task<bool> ExistsWithName(string name, CancellationToken cancellationToken = default);
    void Insert(Faculty faculty);
}
