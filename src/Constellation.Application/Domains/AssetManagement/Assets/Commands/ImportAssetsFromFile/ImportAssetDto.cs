namespace Constellation.Application.Assets.ImportAssetsFromFile;

using System;

public sealed record ImportAssetDto(
    int RowNumber,
    string AssetNumber,
    string SerialNumber,
    string SapNumber,
    string Manufacturer,
    string ModelNumber,
    string ModelDescription,
    string Status,
    string Category,
    DateOnly? PurchaseDate,
    decimal PurchaseCost,
    DateOnly? WarrantyEndDate,
    string LocationCategory,
    string LocationSite,
    string LocationRoom,
    string ResponsibleOfficer,
    string Notes,
    DateOnly? LastSighted);