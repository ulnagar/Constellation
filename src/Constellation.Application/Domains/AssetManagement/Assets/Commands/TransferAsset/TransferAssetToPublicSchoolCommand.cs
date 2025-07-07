namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.TransferAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;
using System;

public sealed record TransferAssetToPublicSchoolCommand(
    AssetNumber AssetNumber,
    string SchoolCode,
    bool CurrentLocation,
    DateOnly ArrivalDate)
    : ICommand;