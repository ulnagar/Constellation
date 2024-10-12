namespace Constellation.Application.Interfaces.Services;

using Constellation.Core.Models;
using System.Threading.Tasks;

public interface IOperationService
{

    Task CreateTeacherEmployedMSTeamAccess(string staffId);
    Task RemoveTeacherEmployedMSTeamAccess(string staffId);
    Task CreateCanvasUserFromStaff(Staff staff);
    Task DisableCanvasUser(string userId);
}