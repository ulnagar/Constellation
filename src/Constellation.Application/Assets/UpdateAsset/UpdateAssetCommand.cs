namespace Constellation.Application.Assets.UpdateAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using System;

public sealed record UpdateAssetCommand(
    AssetNumber AssetNumber,
    string SapEquipmentNumber,
    string Manufacturer,
    string ModelNumber,
    string ModelDescription,
    DateOnly PurchaseDate,
    string PurchaseDocument,
    decimal PurchaseCost,
    DateOnly WarrantyEndDate)
    : ICommand;