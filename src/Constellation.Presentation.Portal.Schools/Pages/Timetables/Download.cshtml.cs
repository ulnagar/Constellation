using Constellation.Application.Features.Portal.School.Timetables.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Presentation.Portal.Schools.Pages.Timetables
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

        public async Task<IActionResult> OnGet(DateTime StartDate, List<string> Students)
        {
            // Get Timetable Data first
            var data = await _mediator.Send(new GetStudentTimetableDataQuery { StudentId = StudentId });

            // We only have one student, so just download that file.
            var file = await _mediator.Send(new GetStudentTimetableExportQuery { Data = data });

            return File(file.FileData, file.FileType, file.Name);
        }
    }
}
