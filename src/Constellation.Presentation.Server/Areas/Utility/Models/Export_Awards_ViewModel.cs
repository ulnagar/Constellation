using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Http;

namespace Constellation.Presentation.Server.Areas.Utility.Models
{
    public class Export_Awards_ViewModel : BaseViewModel
    {
        public IFormFile UploadedFile { get; set; }
    }
}
