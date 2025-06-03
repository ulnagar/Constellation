namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.CreateFullAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.ValueObjects;
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