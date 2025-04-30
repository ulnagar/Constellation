namespace Constellation.Application.Assets.ImportAssetsFromFile;

using Abstractions.Messaging;
using System.Collections.Generic;
using System.IO;

public sealed record ImportAssetsFromFileCommand(
    MemoryStream ImportFile)
    : ICommand<List<ImportAssetStatusDto>>;