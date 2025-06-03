namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.AllocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.Students.Identifiers;

public sealed record AllocateAssetToStudentCommand(
    AssetNumber AssetNumber,
    StudentId StudentId)
    : ICommand;