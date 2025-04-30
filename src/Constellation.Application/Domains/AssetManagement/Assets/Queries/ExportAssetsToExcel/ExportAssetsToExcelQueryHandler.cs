namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.ExportAssetsToExcel;

using Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Enums;
using Constellation.Application.Helpers;
using Core.Abstractions.Clock;
using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportAssetsToExcelQueryHandler
: IQueryHandler<ExportAssetsToExcelQuery, FileDto>
{
    private readonly IAssetRepository _assetsRepository;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    private readonly Error _errorNoAssetsToExport = new("Assets.Export.NoAssetsFound", "No Assets found to export");
    private readonly Error _errorDocumentServiceFailed = new("Assets.Export.DocumentServiceFailed", "Document Service failed to create document");

    public ExportAssetsToExcelQueryHandler(
        IAssetRepository assetsRepository,
        IExcelService excelService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetsRepository = assetsRepository;
        _excelService = excelService;
        _dateTime = dateTime;
        _logger = logger.ForContext<ExportAssetsToExcelQuery>();
    }

    public async Task<Result<FileDto>> Handle(ExportAssetsToExcelQuery request, CancellationToken cancellationToken)
    {
        List<Asset> assets = request.Filter switch
        {
            AssetFilter.All => await _assetsRepository.GetAll(cancellationToken),
            AssetFilter.Disposed => await _assetsRepository.GetAllByStatus(AssetStatus.Disposed, cancellationToken),
            _ => await _assetsRepository.GetAllActive(cancellationToken)
        };

        if (assets.Count == 0)
        {
            _logger
                .ForContext(nameof(ExportAssetsToExcelQuery), request, true)
                .ForContext(nameof(Error), _errorNoAssetsToExport, true)
                .Warning("Failed to export Assets to Excel");

            return Result.Failure<FileDto>(_errorNoAssetsToExport);
        }

        MemoryStream stream = await _excelService.CreateAssetExportFile(assets, cancellationToken);

        if (stream is null)
        {
            _logger
                .ForContext(nameof(ExportAssetsToExcelQuery), request, true)
                .ForContext(nameof(Error), _errorDocumentServiceFailed, true)
                .Warning("Failed to export Assets to Excel");

            return Result.Failure<FileDto>(_errorDocumentServiceFailed);
        }

        FileDto response = new()
        {
            FileData = stream.ToArray(),
            FileName = $"Assets Export - {_dateTime.Today:O}.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };
        
        return response;
    }
}
