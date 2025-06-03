namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.SightAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;
using Core.Models.StaffMembers.Identifiers;
using System;

public sealed record SightAssetCommand(
    AssetNumber AssetNumber,
    StaffId StaffId,
    DateTime SightedAt,
    string Note)
    : ICommand;
