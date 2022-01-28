using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authorization;

namespace Constellation.Presentation.Server.Areas.Home.Pages
{
    [Authorize]
    public class DashboardModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardModel(IUnitOfWork unitOfWork)
            : base()
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet()
        {
            GetClasses(_unitOfWork);
        }
    }
}
