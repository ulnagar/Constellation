using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth
{
    [Roles(AuthRoles.Admin)]
    public class RolesModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public RolesModel(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
            : base()
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            
            Roles = new List<RolesWithUsersDto>();
        }

        public ICollection<RolesWithUsersDto> Roles { get; set; }

        public class RolesWithUsersDto
        {
            public RolesWithUsersDto()
            {
                Members = new List<AppUser>();
            }

            public Guid RoleId { get; set; }
            public string RoleName { get; set; }
            public ICollection<AppUser> Members { get; set; }
        }

        public async Task<IActionResult> OnGet()
        {
            await GetClasses(_unitOfWork);

            var roles = _roleManager.Roles.ToList();
            var users = _userManager.Users.ToList();

            foreach (var role in roles)
            {
                var viewModel = new RolesWithUsersDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                foreach (var user in users)
                {
                    var isInRole = await _userManager.IsInRoleAsync(user, role.Name);

                    if (isInRole)
                    {
                        viewModel.Members.Add(user);
                    }
                }

                Roles.Add(viewModel);
            }

            return Page();
        }
    }
}
