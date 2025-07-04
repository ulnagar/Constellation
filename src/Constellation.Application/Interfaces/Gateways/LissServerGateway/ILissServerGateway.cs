namespace Constellation.Application.Interfaces.Gateways.LissServerGateway;

using Constellation.Application.Interfaces.Gateways.LissServerGateway.Models;
using System.Threading;
using System.Threading.Tasks;

public interface ILissServerGateway
{
    Task<ILissResponse> PublishStudents(object[] request, CancellationToken cancellationToken = default);
    Task<ILissResponse> PublishTimetable(object[] request, CancellationToken cancellationToken = default);
    Task<ILissResponse> PublishTeachers(object[] request, CancellationToken cancellationToken = default);
    Task<ILissResponse> PublishClassMemberships(object[] request, CancellationToken cancellationToken = default);
    Task<ILissResponse> PublishClasses(object[] request, CancellationToken cancellationToken = default);
    Task<ILissResponse> PublishDailyData(object[] request, CancellationToken cancellationToken = default);
}