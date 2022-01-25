using Constellation.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<IdentityResult> CreateUser(UserTemplateDto userDetails);
        Task<IdentityResult> UpdateUser(string username, UserTemplateDto newUser);
        Task<IdentityResult> AddUserToRole(UserTemplateDto user, string group);
        Task<IdentityResult> RemoveUserFromRole(UserTemplateDto user, string group);
        Task<IdentityResult> RemoveUser(UserTemplateDto user);
        Task<IdentityResult> ChangeUserPassword(string userId, string password, string newPassword);
        Task<IdentityResult> ChangeUserPassword(string userId, string newPassword);
        Task RepairStaffUserAccounts();
        Task AuditSchoolContactUsers();
        Task<UserAuditDto> VerifyContactAccess(string email);
        Task RepairSchoolContactUser(int schoolContactId);
    }
}
