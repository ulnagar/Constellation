namespace Constellation.Application.Compliance.ExportWellbeingReport;

using Abstractions.Messaging;
using Core.Shared;
using DTOs;
using Interfaces.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportWellbeingReportCommandHandler
: ICommandHandler<ExportWellbeingReportCommand, FileDto>
{
    private readonly IExcelService _excelService;

    public ExportWellbeingReportCommandHandler(
        IExcelService excelService)
    {
        _excelService = excelService;
    }

    public async Task<Result<FileDto>> Handle(ExportWellbeingReportCommand request, CancellationToken cancellationToken)
    {
        MemoryStream stream = await _excelService.CreateWellbeingExportFile(request.Records, cancellationToken);

        string fileName = $"Wellbeing Report.xlsx";

        FileDto result = new FileDto
        {
            FileName = fileName,
            FileData = stream.ToArray(),
            FileType = "application/vnd.ms-excel"
        };

        return result;
    }
}
