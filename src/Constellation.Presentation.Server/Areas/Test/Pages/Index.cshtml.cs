using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization.Infrastructure;
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

        public IndexModel(IUnitOfWork unitOfWork, IMediator mediator, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _authService = authService;
        }

        public List<Claim> Claims { get; set; }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);

            Claims = User.Claims.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _authService.RepairStaffUserAccounts();

            return Page();
        }
    }
}
