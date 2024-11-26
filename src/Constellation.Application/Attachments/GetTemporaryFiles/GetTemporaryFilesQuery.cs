namespace Constellation.Application.Attachments.GetTemporaryFiles;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetTemporaryFilesQuery()
    : IQuery<List<ExternalReportTemporaryFileResponse>>;