using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(IUnitOfWork unitOfWork, IMediator mediator, IAuthService authService, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _authService = authService;
            _userManager = userManager;
        }

        public List<Claim> Claims { get; set; } = new();
        public List<string> Roles { get; set; } = new();

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);

            Claims = User.Claims.ToList();

            if (User.Identity is not null)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                Roles = (List<string>)await _userManager.GetRolesAsync(user);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _authService.RepairStaffUserAccounts();

            return Page();
        }
    }
}
