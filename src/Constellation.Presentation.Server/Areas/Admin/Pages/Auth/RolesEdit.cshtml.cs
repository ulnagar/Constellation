using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
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
    public class RolesEditModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public RolesEditModel(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
            : base()
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;

            Members = new List<AppUser>();
            NonMembers = new List<AppUser>();
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        [BindProperty]
        public Guid UserId { get; set; }

        public AppRole Role { get; set; }
        public ICollection<AppUser> Members { get; set; }
        public ICollection<AppUser> NonMembers { get; set; }

        public async Task<IActionResult> OnGet()
        {
            await GetClasses(_unitOfWork);

            Role = await _roleManager.FindByIdAsync(Id.ToString());
            var users = _userManager.Users.ToList();

            foreach (var user in users)
            {
                var isInRole = await _userManager.IsInRoleAsync(user, Role.Name);
                if (isInRole)
                    Members.Add(user);
                else
                    NonMembers.Add(user);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddUser()
        {
            if (UserId == Guid.Empty)
            {
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(UserId.ToString());

            var role = await _roleManager.FindByIdAsync(Id.ToString());

            await _userManager.AddToRoleAsync(user, role.Name);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveUser()
        {
            if (UserId == Guid.Empty)
            {
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(UserId.ToString());

            var role = await _roleManager.FindByIdAsync(Id.ToString());

            await _userManager.RemoveFromRoleAsync(user, role.Name);

            return RedirectToPage();
        }
    }
}
