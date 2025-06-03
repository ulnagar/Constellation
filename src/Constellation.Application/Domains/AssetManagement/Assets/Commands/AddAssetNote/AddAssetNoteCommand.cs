namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.AddAssetNote;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;

public sealed record AddAssetNoteCommand(
    AssetNumber AssetNumber,
    string Note)
    : ICommand;