namespace Constellation.Infrastructure.Features.Portal.School.Timetables.Queries;

using Application.DTOs;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Constellation.Application.Interfaces.Services;
using System.Net.Mime;

public class GetStudentTimetableExportQueryHandler : IRequestHandler<GetStudentTimetableExportQuery, FileDto>
{
    private readonly IRazorViewToStringRenderer _renderService;
    private readonly IPDFService _pdfService;

    public GetStudentTimetableExportQueryHandler(IRazorViewToStringRenderer renderService, IPDFService pdfService)
    {
        _renderService = renderService;
        _pdfService = pdfService;
    }

    public async Task<FileDto> Handle(GetStudentTimetableExportQuery request, CancellationToken cancellationToken)
    {
        string fileName = $"{request.Data.StudentName} Timetable.pdf";

        string headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Timetable/StudentTimetableHeader.cshtml", request.Data);
        string htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Timetable/Timetable.cshtml", request.Data);

        MemoryStream pdfStream = _pdfService.StringToPdfStream(htmlString, headerString);

        FileDto result = new FileDto
        {
            FileName = fileName,
            FileData = pdfStream.ToArray(),
            FileType = MediaTypeNames.Application.Pdf
        };

        return result;
    }
}