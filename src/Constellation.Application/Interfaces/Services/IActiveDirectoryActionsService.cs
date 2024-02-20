namespace Constellation.Application.Interfaces.Services;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IActiveDirectoryActionsService
{
    Task<List<string>> GetLinkedSchoolsFromAD(string emailAddress);
    (string FirstName, string LastName) GetUserDetailsFromAD(string emailAddress);
}