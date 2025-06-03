namespace Constellation.Application.Interfaces.Services;

using Core.Models.StaffMembers;
using System.Threading.Tasks;

public interface IOperationService
{

    Task CreateTeacherEmployedMSTeamAccess(string staffId);
    Task RemoveTeacherEmployedMSTeamAccess(string staffId);
    Task CreateCanvasUserFromStaff(StaffMember staff);
    Task DisableCanvasUser(string userId);
}