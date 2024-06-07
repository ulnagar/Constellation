namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models.Students.Repositories;
using Core.Models.StaffMembers.Repositories;
using System.Threading;
using System.Threading.Tasks;

public interface IUnitOfWork
{
    IAdobeConnectOperationsRepository AdobeConnectOperations { get; set; }
    IAdobeConnectRoomRepository AdobeConnectRooms { get; set; }
    IMSTeamOperationsRepository MSTeamOperations { get; set; }
    ISchoolRepository Schools { get; set; }
    IStaffRepository Staff { get; set; }
    IStudentRepository Students { get; set; }
    ITimetablePeriodRepository Periods { get; set; }


    void Add<TEntity>(TEntity entity) where TEntity : class;
    Task CompleteAsync(CancellationToken token);
    Task CompleteAsync();
}
