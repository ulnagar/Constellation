using Constellation.Application.Features.Attendance.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Constellation.Presentation.Portal.Schools.Pages.Absences
{
    public class DownloadModel : PageModel
    {
        private readonly IMediator _mediator;

        public DownloadModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty(SupportsGet = true)]
        public string StudentId { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var file = await _mediator.Send(new GetStudentAttendanceReportQuery { StudentId = StudentId, StartDate = StartDate });

            return File(file.FileData, file.FileType, file.Name);
        }
    }
}
