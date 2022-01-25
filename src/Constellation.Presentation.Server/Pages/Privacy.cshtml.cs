using Constellation.Presentation.Server.BaseModels;
using Microsoft.Extensions.Logging;

namespace Constellation.Presentation.Server.Pages
{
    public class PrivacyModel : BasePageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
            : base()
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
