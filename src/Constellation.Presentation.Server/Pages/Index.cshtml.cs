using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
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
