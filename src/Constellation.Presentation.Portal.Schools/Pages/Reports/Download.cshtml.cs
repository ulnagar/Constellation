using Constellation.Application.Features.Portal.School.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Constellation.Presentation.Portal.Schools.Pages.Reports
{
    public class DownloadModel : PageModel
    {
        private readonly IMediator _mediator;

        public DownloadModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty(SupportsGet = true)]
        public string ReportId { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var file = await _mediator.Send(new GetFileFromDatabaseQuery { LinkType = Core.Models.StoredFile.StudentReport, LinkId = ReportId.ToString() });

            return File(file.FileData, file.FileType, file.Name);
        }
    }
}
