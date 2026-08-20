namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.ExportAssetsToExcel;

using Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Enums;
using Constellation.Application.Helpers;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Services;
using Interfaces.Services.Excel;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportAssetsToExcelQueryHandler
: IQueryHandler<ExportAssetsToExcelQuery, FileDto>
{
    private readonly IAssetRepository _assetsRepository;
    private readonly IExcelWriter _writer;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;
    
    public ExportAssetsToExcelQueryHandler(
        IAssetRepository assetsRepository,
        IExcelWriter writer,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetsRepository = assetsRepository;
        _writer = writer;
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
                .ForContext(nameof(Error), AssetErrors.NoneFound, true)
                .Warning("Failed to export Assets to Excel");

            return Result.Failure<FileDto>(AssetErrors.NoneFound);
        }

        IExcelWorkbook workbook = _writer.CreateWorkbook();
        IExcelWorksheet sheet = _writer.AddWorksheet(workbook, "Sheet 1");

        _writer.WriteRange(sheet, 2, assets.OrderBy(entry => entry.AssetNumber),
            new("Asset Number", a => a.AssetNumber),
            new("Serial Number", a => a.SerialNumber),
            new("SAP Equipment Number", a => a.SapEquipmentNumber),
            new("Manufacturer", a => a.Manufacturer),
            new("Model Number", a => a.ModelNumber),
            new ("Model Description", a => a.ModelDescription),
            new ("Status", a => a.Status.Name),
            new("Device Category", a => a.Category.Name),
            new("Purchase Date", a => a.PurchaseDate == DateOnly.MinValue ? null : a.PurchaseDate, ExcelColumnFormat.Date),
            new ("Purchase Cost", a => a.PurchaseCost, ExcelColumnFormat.Financial),
            new ("Warranty End Date", a => a.WarrantyEndDate == DateOnly.MinValue ? null : a.WarrantyEndDate, ExcelColumnFormat.Date),
            new ("Location Category", a => a.CurrentLocation?.Category.Name ?? string.Empty),
            new("Location Site", a => a.CurrentLocation?.Site ?? string.Empty),
            new ("Location Room", a => a.CurrentLocation?.Room ?? string.Empty),
            new("Responsible Officer", a => a.CurrentAllocation?.ResponsibleOfficer ?? string.Empty),
            new("Last Seen", a => a.LastSighting?.SightedAt, ExcelColumnFormat.Date),
            new("Last Seen By", a => a.LastSighting?.SightedBy ?? string.Empty),
            new ("Notes", a => string.Join("\n", a.Notes
                .OrderByDescending(note => note.CreatedAt)
                .Select(note => $"{note.CreatedAt} - {note.CreatedBy} - {note.Message}")), ExcelColumnFormat.List));

        _writer.ApplyHeaderStyle(sheet, 1);
        _writer.AddAutoFilter(sheet);
        _writer.AutoFitColumns(sheet);

        FileDto response = new()
        {
            FileData = _writer.GetAsByteArray(workbook),
            FileName = $"Assets Export - {_dateTime.Today:O}.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };
        
        return response;
    }
}
