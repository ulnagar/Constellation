using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using MediatR;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Timetables.Queries
{
    public class GetStudentTimetableExportQueryHandler : IRequestHandler<GetStudentTimetableExportQuery, StoredFile>
    {
        private readonly IRazorViewToStringRenderer _renderService;
        private readonly IPDFService _pdfService;

        public GetStudentTimetableExportQueryHandler(IRazorViewToStringRenderer renderService, IPDFService pdfService)
        {
            _renderService = renderService;
            _pdfService = pdfService;
        }

        public async Task<StoredFile> Handle(GetStudentTimetableExportQuery request, CancellationToken cancellationToken)
        {
            var fileName = $"{request.Data.StudentName} Timetable.pdf";

            var headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Timetable/StudentTimetableHeader.cshtml", request.Data);
            var htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Timetable/Timetable.cshtml", request.Data);

            var pdfStream = _pdfService.StringToPdfStream(htmlString, headerString);

            var result = new StoredFile
            {
                Name = fileName,
                FileData = pdfStream.ToArray(),
                FileType = MediaTypeNames.Application.Pdf
            };

            return result;
        }
    }
}
