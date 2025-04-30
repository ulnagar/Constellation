namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.ExportAssetsToExcel;

using Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Enums;
using DTOs;
using Enums;

public sealed record ExportAssetsToExcelQuery(
    AssetFilter Filter)
    : IQuery<FileDto>;