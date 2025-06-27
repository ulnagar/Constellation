namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.ExportStocktakeSightingsAndDifferences;

using Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Queries.ExportAssetsToExcel;
using Constellation.Application.Helpers;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Enums;
using Core.Models.Stocktake.Errors;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportStocktakeSightingsAndDifferencesQueryHandler
: IQueryHandler<ExportStocktakeSightingsAndDifferencesQuery, FileDto>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    private readonly Error _errorDocumentServiceFailed = new("Stocktake.Export.DocumentServiceFailed", "Document Service failed to create document");
    
    public ExportStocktakeSightingsAndDifferencesQueryHandler(
        IStocktakeRepository stocktakeRepository,
        IExcelService excelService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _excelService = excelService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<ExportStocktakeSightingsAndDifferencesQuery>();
    }

    public async Task<Result<FileDto>> Handle(ExportStocktakeSightingsAndDifferencesQuery request, CancellationToken cancellationToken)
    {
        StocktakeEvent @event = await _stocktakeRepository.GetById(request.EventId, cancellationToken);

        if (@event is null)
        {
            _logger
                .ForContext(nameof(ExportStocktakeSightingsAndDifferencesQuery), request, true)
                .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.EventId), true)
                .Warning("Failed to export Stocktake Sightings and Differences report");

            return Result.Failure<FileDto>(StocktakeEventErrors.EventNotFound(request.EventId));
        }

        List<StocktakeSightingWithDifferenceResponse> items = [];

        foreach (StocktakeSighting sighting in @event.Sightings)
        {
            if (sighting.IsCancelled)
                continue;

            DifferenceCategory difference =
                @event.Differences.FirstOrDefault(entry => entry.SightingId == sighting.Id)?.Category ??
                DifferenceCategory.None;

            items.Add(new(
                sighting.AssetNumber,
                sighting.SerialNumber,
                sighting.Description,
                sighting.LocationCategory,
                sighting.LocationName,
                sighting.UserType,
                sighting.UserName,
                sighting.Comment,
                sighting.SightedBy,
                sighting.SightedAt,
                difference));
        }

        if (items.Count == 0)
        {
            return Result.Failure<FileDto>(IntegrationErrors.Exports.NoItemsToInclude);
        }

        MemoryStream stream = await _excelService.CreateStocktakeSightingsReport(items, cancellationToken);

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
            FileName = $"Stocktake Export - {_dateTime.Today:O}.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return response;
    }
}
