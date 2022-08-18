using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly UserManager<AppUser> _userManager;

        public IdentityRepository(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ICollection<AppUser>> UsersInRole(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }
    }
}
