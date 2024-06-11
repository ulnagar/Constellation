namespace Constellation.Application.Assets.CreateFullAsset;

using Abstractions.Messaging;
using Core.Models.Assets.Enums;
using Core.Models.Assets.ValueObjects;
using System;

public sealed record CreateFullAssetCommand(
    AssetNumber AssetNumber,
    string SerialNumber,
    string Manufacturer,
    string ModelNumber,
    string ModelDescription,
    AssetCategory Category,
    string SapEquipmentNumber,
    DateOnly PurchaseDate,
    string PurchaseDocument,
    decimal PurchaseCost,
    DateOnly WarrantyEndDate)
    : ICommand;