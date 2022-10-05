namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IActiveDirectoryGateway
{
    Task<UserTemplateDto> GetUserDetailsFromActiveDirectory(string email);
    Task<bool> VerifyUserCredentialsAgainstActiveDirectory(string email, string password);
    Task<List<string>> GetLinkedSchoolsFromActiveDirectory(string email);
}
