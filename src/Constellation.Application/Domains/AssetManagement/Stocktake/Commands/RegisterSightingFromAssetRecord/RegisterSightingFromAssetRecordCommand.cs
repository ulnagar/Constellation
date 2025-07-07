namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterSightingFromAssetRecord;

using Abstractions.Messaging;
using Core.Models.Assets.Identifiers;
using Core.Models.Stocktake.Identifiers;

public sealed record RegisterSightingFromAssetRecordCommand(
    StocktakeEventId EventId,
    AssetId AssetId,
    string Comment)
    : ICommand;