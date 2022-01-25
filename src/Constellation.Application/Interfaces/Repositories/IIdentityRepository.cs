using Constellation.Application.Models.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IIdentityRepository
    {
        Task<ICollection<AppUser>> UsersInRole(string role);
    }
}
