namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.ImportAssetsFromFile;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;
using System.IO;

public sealed record ImportAssetsFromFileCommand(
    MemoryStream ImportFile)
    : ICommand<List<ImportAssetStatusDto>>;