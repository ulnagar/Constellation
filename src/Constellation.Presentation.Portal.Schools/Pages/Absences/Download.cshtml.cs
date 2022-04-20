using Constellation.Application.Features.Attendance.Queries;
using Constellation.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
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
        public List<string> Students { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; }

        public async Task<IActionResult> OnGet(DateTime StartDate, List<string> Students)
        {
            if (Students.Count > 1)
            {
                var reports = new List<StoredFile>();

                // We need to loop each student id and collate the report into a zip file.
                foreach (var student in Students)
                {
                    var report = await _mediator.Send(new GetStudentAttendanceReportQuery { StudentId = student, StartDate = StartDate });
                    reports.Add(report);
                }

                // Zip all reports
                using var memoryStream = new MemoryStream();
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                {
                    foreach (var file in reports)
                    {
                        var zipArchiveEntry = zipArchive.CreateEntry(file.Name);
                        using var streamWriter = new StreamWriter(zipArchiveEntry.Open());
                        var fileData = file.FileData;
                        streamWriter.BaseStream.Write(fileData, 0, fileData.Length);
                    }
                }

                var attachmentStream = new MemoryStream(memoryStream.ToArray());
                return File(attachmentStream.ToArray(), MediaTypeNames.Application.Zip, "Attendance Reports.zip");
            }
            else
            {
                // We only have one student, so just download that file.
                var file = await _mediator.Send(new GetStudentAttendanceReportQuery { StudentId = Students.First(), StartDate = StartDate });

                return File(file.FileData, file.FileType, file.Name);
            }
        }
    }
}
