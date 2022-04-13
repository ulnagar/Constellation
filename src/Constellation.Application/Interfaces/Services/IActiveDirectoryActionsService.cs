using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IActiveDirectoryActionsService
    {
        Task<List<string>> GetLinkedSchoolsFromAD(string emailAddress);
    }
}
