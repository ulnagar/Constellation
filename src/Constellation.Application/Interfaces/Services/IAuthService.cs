namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.DTOs;
using Core.Models.SchoolContacts.Identifiers;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

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
    Task RepairSchoolContactUser(SchoolContactId schoolContactId);
    Task AuditParentUsers(CancellationToken cancellationToken = default);
    Task AuditAllUsers(CancellationToken cancellationToken);
}
