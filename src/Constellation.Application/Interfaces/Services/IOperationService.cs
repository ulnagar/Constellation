namespace Constellation.Application.Interfaces.Services;

using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using System.Threading.Tasks;

public interface IOperationService
{
    Task RemoveTeacherEmployedMSTeamAccess(StaffId staffId);
    Task DisableCanvasUser(string userId);
}