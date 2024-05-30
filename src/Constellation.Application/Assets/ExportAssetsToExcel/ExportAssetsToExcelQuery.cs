namespace Constellation.Application.Assets.ExportAssetsToExcel;

using Abstractions.Messaging;
using DTOs;
using Enums;

public sealed record ExportAssetsToExcelQuery(
    AssetFilter Filter)
    : IQuery<FileDto>;