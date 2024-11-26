namespace Constellation.Application.Attachments.ProcessPATReportZipFile;

using Abstractions.Messaging;
using System.Collections.Generic;
using System.IO;

public sealed record ProcessPATReportZipFileCommand(
    MemoryStream ArchiveFile)
    : ICommand<List<string>>;