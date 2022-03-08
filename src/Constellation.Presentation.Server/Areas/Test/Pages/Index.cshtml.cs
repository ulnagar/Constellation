using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly ILinkShortenerGateway _gateway;
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(ILinkShortenerGateway gateway, IUnitOfWork unitOfWork)
        {
            _gateway = gateway;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public string LinkToShorten { get; set; }
        public string ShortLink { get; set; }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ShortLink = await _gateway.ShortenURL(LinkToShorten);

            return Page();
        }
    }
}
